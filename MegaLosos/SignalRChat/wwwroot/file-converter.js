function getFileType(file) {
    const type = file.type;
    const name = file.name.toLowerCase();
    
    // Images
    if (type.startsWith('image/')) return 'image';
    if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg'].some(ext => name.endsWith(ext))) return 'image';
    
    // Videos
    if (type.startsWith('video/')) return 'video';
    if (['.mp4', '.avi', '.mov', '.wmv', '.flv', '.webm', '.mkv'].some(ext => name.endsWith(ext))) return 'video';
    
    // Audio
    if (type.startsWith('audio/')) return 'audio';
    if (['.mp3', '.wav', '.ogg', '.flac', '.aac'].some(ext => name.endsWith(ext))) return 'audio';
    
    // Archives
    if (['.zip', '.rar', '.7z', '.tar', '.gz'].some(ext => name.endsWith(ext))) return 'archive';
    
    // Documents
    if (['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx'].some(ext => name.endsWith(ext))) return 'document';
    
    // Text files
    if (type === 'text/plain' || name.endsWith('.txt')) return 'text';
    
    // JSON
    if (type === 'application/json' || name.endsWith('.json')) return 'json';
    
    return 'other';
}

//formdata

class FileUploadManager {
    constructor(options = {}) {
        this.maxFiles = options.maxFiles || 5;
        this.maxSizeBytes = options.maxSizeMB ? options.maxSizeMB * 1024 * 1024 : 10 * 1024 * 1024;
        this.allowedTypes = options.allowedTypes || null;

        this.files = [];
            
        this.fileInput = document.getElementById('fileInp');
        this.fileList = document.getElementById('fileList');
        this.stats = document.getElementById('stats');
        this.messages = document.getElementById('fileMessages');
        this.uploadBtn = document.getElementById('fileUploadBtn');

        this.setupEventListeners();
    }

    setupEventListeners() {
        this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        this.uploadBtn.addEventListener('click', () => this.uploadFiles());
        
        const dropZone = document.querySelector('.file-input-wrapper');
        dropZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            dropZone.style.borderColor = '#4CAF50';
            dropZone.style.background = '#e8f5e9';
        });
        
        dropZone.addEventListener('dragleave', () => {
            dropZone.style.borderColor = '#ccc';
            dropZone.style.background = '';
        });
        
        dropZone.addEventListener('drop', (e) => {
            e.preventDefault();
            dropZone.style.borderColor = '#ccc';
            dropZone.style.background = '';
            this.handleFiles(e.dataTransfer.files);
        });
    }
    handleFileSelect(event) {
        console.log(event);
        this.handleFiles(event.target.files);
        this.fileInput.value = '';
        this.sendFile(event.target.files[0]);
    }
    
    sendFile(file) {
        console.log(file.name);
        const formdata = new FormData();
        formdata.append("file", file);
        const url = "api/upload";
        Api.postFile(url, formdata, () => {
            console.log('file sent successfully');
        });
    }

    handleFiles(fileList) {
        const newFiles = Array.from(fileList);
        
        if (this.files.length + newFiles.length > this.maxFiles) {
            return;
        }
        
        const totalSize = this.getTotalSize() + newFiles.reduce((sum, f) => sum + f.size, 0);
        if (totalSize > this.maxSizeBytes) {
            const currentMB = (this.getTotalSize() / (1024 * 1024)).toFixed(2);
            const newMB = (newFiles.reduce((sum, f) => sum + f.size, 0) / (1024 * 1024)).toFixed(2);
            return;
        }
        
        for (const file of newFiles) {
            if (file.size > this.maxSizeBytes) {
                return;
            }
        }
        
        if (this.allowedTypes) {
            for (const file of newFiles) {
                if (!this.isFileTypeAllowed(file)) {
                    return;
                }
            }
        }
    }
    
    isFileTypeAllowed(file) {
        return this.allowedTypes.some(type => {
            if (type.startsWith('.')) {
                return file.name.toLowerCase().endsWith(type);
            }
            return file.type.startsWith(type);
        });
    }
    
    getTotalSize() {
        return this.files.reduce((sum, f) => sum + f.size, 0);
    }
    
    getFileType(file) {
        const type = file.type;
        const name = file.name.toLowerCase();
        
        if (type.startsWith('image/')) return 'image';
        if (type.startsWith('video/')) return 'video';
        if (type.startsWith('audio/')) return 'audio';
        if (type === 'text/plain' || name.endsWith('.txt')) return 'text';
        if (type === 'application/json' || name.endsWith('.json')) return 'json';
        if (['.zip', '.rar', '.7z', '.tar', '.gz'].some(ext => name.endsWith(ext))) return 'archive';
        return 'other';
    }
    
    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }
    
    async uploadFiles() {
        if (this.files.length === 0) {
            return;
        }
        
        const totalSize = this.getTotalSize();
        if (totalSize > this.maxSizeBytes) {
            return;
        }
        
        this.uploadBtn.disabled = true;
        this.uploadBtn.textContent = 'Uploading...';
        
        try {
            // Method 1: Send as FormData
            const formData = new FormData();
            formData.append('totalFiles', this.files.length);
            
            for (const file of this.files) {
                formData.append('files', file);
            }
            
            const response = await fetch('/upload-multiple', {
                method: 'POST',
                body: formData
            });
            
            const result = await response.json();
            
            if (result.success) {
                this.files = [];
            } else {
            }
        } catch (error) {
        } finally {
            this.uploadBtn.disabled = false;
            this.uploadBtn.textContent = 'Upload Files';
        }
    }
    
    readFileAsDataURL(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => resolve(e.target.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }
}

// Initialize with custom limits
const uploadManager = new FileUploadManager({
    maxFiles: 5,
    maxSizeMB: 10,
    allowedTypes: null // Set to ['image/', '.pdf'] to limit types
});
