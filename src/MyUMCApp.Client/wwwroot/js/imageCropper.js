export function initCropper(imageId, options) {
    const image = document.getElementById(imageId);
    if (!image) {
        throw new Error(`Image element with id ${imageId} not found`);
    }

    return new Cropper(image, options);
}

export function getCroppedImage(cropper) {
    return cropper.getCroppedCanvas({
        width: 400,
        height: 400,
        fillColor: '#fff',
        imageSmoothingEnabled: true,
        imageSmoothingQuality: 'high'
    }).toDataURL('image/jpeg', 0.9);
} 