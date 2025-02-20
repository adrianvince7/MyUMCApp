const filters = {
    none: '',
    grayscale: 'grayscale(100%)',
    sepia: 'sepia(100%)',
    vintage: 'sepia(50%) contrast(150%)',
    warm: 'saturate(150%) brightness(110%)',
    cool: 'saturate(80%) hue-rotate(30deg)',
    dramatic: 'contrast(150%) brightness(90%)',
    fade: 'brightness(110%) saturate(80%) opacity(90%)'
};

let originalImage = null;
let currentAdjustments = {
    brightness: 100,
    contrast: 100,
    saturation: 100
};

export function initFilters(imageId) {
    const image = document.getElementById(imageId);
    if (!image) {
        throw new Error(`Image element with id ${imageId} not found`);
    }

    // Store original image for reset
    originalImage = image.cloneNode(true);
}

export function applyFilter(imageId, filterName) {
    const image = document.getElementById(imageId);
    if (!image) return;

    const filter = filters[filterName] || '';
    updateImageStyle(image);
}

export function adjustBrightness(imageId, value) {
    currentAdjustments.brightness = value;
    const image = document.getElementById(imageId);
    if (!image) return;
    
    updateImageStyle(image);
}

export function adjustContrast(imageId, value) {
    currentAdjustments.contrast = value;
    const image = document.getElementById(imageId);
    if (!image) return;
    
    updateImageStyle(image);
}

export function adjustSaturation(imageId, value) {
    currentAdjustments.saturation = value;
    const image = document.getElementById(imageId);
    if (!image) return;
    
    updateImageStyle(image);
}

export function resetImage(imageId) {
    const image = document.getElementById(imageId);
    if (!image || !originalImage) return;

    image.style.filter = '';
    currentAdjustments = {
        brightness: 100,
        contrast: 100,
        saturation: 100
    };
}

export function getProcessedImage(imageId) {
    const image = document.getElementById(imageId);
    if (!image) return null;

    // Create a canvas to apply the filters
    const canvas = document.createElement('canvas');
    canvas.width = image.naturalWidth;
    canvas.height = image.naturalHeight;
    const ctx = canvas.getContext('2d');

    // Apply current filter and adjustments
    ctx.filter = image.style.filter;
    ctx.drawImage(image, 0, 0);

    // Return as data URL
    return canvas.toDataURL('image/jpeg', 0.9);
}

function updateImageStyle(image) {
    const adjustments = [
        `brightness(${currentAdjustments.brightness}%)`,
        `contrast(${currentAdjustments.contrast}%)`,
        `saturate(${currentAdjustments.saturation}%)`
    ];

    const currentFilter = Object.entries(filters)
        .find(([name, value]) => image.style.filter.includes(value));
    
    if (currentFilter) {
        adjustments.push(currentFilter[1]);
    }

    image.style.filter = adjustments.join(' ');
} 