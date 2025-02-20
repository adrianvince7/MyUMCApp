const AWS = require('aws-sdk');
const Sharp = require('sharp');

const dynamodb = new AWS.DynamoDB.DocumentClient();
const TABLE_NAME = process.env.METADATA_TABLE_NAME;

exports.handler = async (event) => {
    const { request, response } = event.Records[0].cf;
    
    // Only process successful image responses
    if (!response.status.startsWith('2') || !response.headers['content-type'][0].value.startsWith('image/')) {
        return response;
    }

    try {
        const startTime = process.hrtime();

        // Parse query parameters
        const params = new URLSearchParams(request.querystring);
        const width = parseInt(params.get('w')) || null;
        const height = parseInt(params.get('h')) || null;
        const quality = parseInt(params.get('q')) || 80;

        // Get image data from response
        const buffer = Buffer.from(response.body.data, 'base64');
        const originalSize = buffer.length;
        
        // Process image with Sharp
        let image = Sharp(buffer);

        // Extract metadata
        const metadata = await image.metadata();
        const stats = {
            originalSize,
            format: metadata.format,
            width: metadata.width,
            height: metadata.height,
            space: metadata.space,
            channels: metadata.channels,
            depth: metadata.depth,
            density: metadata.density,
            hasAlpha: metadata.hasAlpha,
            orientation: metadata.orientation,
            exif: metadata.exif ? await extractExif(metadata.exif) : {}
        };

        // Resize if needed
        if (width || height) {
            image = image.resize(width, height, {
                fit: 'cover',
                withoutEnlargement: true
            });
        }

        // Optimize based on image type
        const contentType = response.headers['content-type'][0].value;
        let optimizedBuffer;
        const optimizationSettings = {
            quality,
            progressive: true
        };

        switch (contentType) {
            case 'image/jpeg':
                optimizedBuffer = await image
                    .jpeg(optimizationSettings)
                    .toBuffer();
                break;
            case 'image/png':
                optimizedBuffer = await image
                    .png(optimizationSettings)
                    .toBuffer();
                break;
            case 'image/webp':
                optimizedBuffer = await image
                    .webp({ quality })
                    .toBuffer();
                break;
            default:
                return response;
        }

        // Calculate compression statistics
        const endTime = process.hrtime(startTime);
        const compressionStats = {
            originalSize,
            compressedSize: optimizedBuffer.length,
            quality,
            format: contentType.split('/')[1],
            processingTime: (endTime[0] * 1e9 + endTime[1]) / 1e6, // Convert to milliseconds
            isOptimized: true,
            optimizationSettings
        };

        // Store metadata in DynamoDB
        const imageKey = decodeURIComponent(request.uri.substring(1)); // Remove leading slash
        await storeMetadata({
            id: generateId(imageKey),
            fileName: imageKey.split('/').pop(),
            contentType,
            ...stats,
            compressionStats,
            url: `https://${request.headers.host[0].value}${request.uri}`,
            thumbnailUrl: `https://${request.headers.host[0].value}${request.uri}?w=200&q=60`,
            uploadedAt: new Date().toISOString()
        });

        // Update response with optimized image
        response.body = optimizedBuffer.toString('base64');
        response.body.encoding = 'base64';
        response.headers['content-length'] = [{ 
            key: 'Content-Length',
            value: optimizedBuffer.length.toString()
        }];

        // Add cache control headers
        response.headers['cache-control'] = [{
            key: 'Cache-Control',
            value: 'public, max-age=31536000'
        }];

        // Add image optimization headers
        response.headers['x-image-processed'] = [{
            key: 'X-Image-Processed',
            value: 'true'
        }];
        response.headers['x-image-metadata'] = [{
            key: 'X-Image-Metadata',
            value: JSON.stringify({
                dimensions: `${metadata.width}x${metadata.height}`,
                format: metadata.format,
                size: optimizedBuffer.length
            })
        }];

        return response;
    } catch (error) {
        console.error('Error processing image:', error);
        return response;
    }
};

async function extractExif(exifBuffer) {
    try {
        const exif = {};
        const data = exifBuffer.toString('binary');
        
        // Extract basic EXIF data
        const tags = [
            'Make', 'Model', 'DateTimeOriginal', 'ExposureTime',
            'FNumber', 'ISO', 'FocalLength', 'GPSLatitude',
            'GPSLongitude', 'Software'
        ];

        for (const tag of tags) {
            if (data.includes(tag)) {
                const match = data.match(new RegExp(`${tag}=([^\\n]+)`));
                if (match) {
                    exif[tag] = match[1].trim();
                }
            }
        }

        return exif;
    } catch (error) {
        console.error('Error extracting EXIF data:', error);
        return {};
    }
}

async function storeMetadata(metadata) {
    try {
        await dynamodb.put({
            TableName: TABLE_NAME,
            Item: metadata
        }).promise();
    } catch (error) {
        console.error('Error storing metadata:', error);
    }
}

function generateId(key) {
    return require('crypto')
        .createHash('md5')
        .update(key)
        .digest('hex');
} 