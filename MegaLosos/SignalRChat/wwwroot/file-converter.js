function getFileType(filePath) {
    const name = filePath.toLowerCase();
    
    // if (type.startsWith('image/')) return 'image';
    if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg', '.ico'].some(ext => name.endsWith(ext))) return '.tpl-img';
    // if (type.startsWith('video/')) return 'video';
    if (['.mp4', '.webm', '.mov'].some(ext => name.endsWith(ext))) return '.tpl-video';
    
    // if (type.startsWith('audio/')) return 'audio';
    if (['.mp3', '.wav', '.ogg', '.flac', '.aac', '.m4a'].some(ext => name.endsWith(ext))) return '.tpl-audio';
    if (['.pdf', '.txt', '.html', '.htm', '.json', '.xml'].some(ext => name.endsWith(ext))) return '.tpl-iframe';
    if (['.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.rtf', '.odt', '.ods', '.odp'].some(ext => name.endsWith(ext))) return '.tpl-g-iframe';
    
    return '.tpl-download-btn';
    // if (['.zip', '.rar', '.7z', '.tar', '.gz'].some(ext => name.endsWith(ext))) return 'archive';
    // if (['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx'].some(ext => name.endsWith(ext))) return 'document';
    // if (type === 'text/plain' || name.endsWith('.txt')) return 'text';
    // if (type === 'application/json' || name.endsWith('.json')) return 'json';
}
function activateMedia(fileMsg, fileUrl) {
    const type = getFileType(fileUrl);
    
    if (type === '.tpl-img') {
        const img = fileMsg.querySelector(type);
        img.src = fileUrl;
        img.style.display = 'block';
    } 
    
    else if (type === '.tpl-video') {
        const video = fileMsg.querySelector(type);
        video.src = fileUrl;
        video.style.display = 'block';
        video.load();
    } 
    
    else if (type === '.tpl-audio') {
        const audio = fileMsg.querySelector(type);
        audio.src = fileUrl;
        audio.style.display = 'block';
        audio.load();
    } 
    
    else if (type === '.tpl-iframe') {
        const iframe = document.querySelector(type);
        iframe.src = fileUrl;
        iframe.style.display = 'block';
    } 

    else if (type === '.tpl-g-iframe') {
        const giframe = document.querySelector(type);
        giframe.src = fileUrl;
        giframe.style.display = 'block';
    } 
    
    
    // else if (type === 'embed') {  // MOZHET VSE SLOMAT!                                                    !!!!!
        //   const placeholder = document.getElementById('tpl-embed-placeholder');
        //   placeholder.innerHTML = `<embed src="${fileUrl}" type="application/pdf" width="100%" height="500px">`;
        //   placeholder.style.display = 'block';
    // }
}

//formdata

class FileUploadManager {
    constructor(options = {}) {
        this.maxFiles = options.maxFiles || 5;
        this.maxSizeBytes = options.maxSizeMB ? options.maxSizeMB * 1024 * 1024 : 10 * 1024 * 1024;
        this.allowedTypes = options.allowedTypes || null;

        this.allFiles = [];
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
        this.sendFile(this.allFiles[0]);
        this.fileInput.value = '';
    }
    
    sendFile(file) {
        const formdata = new FormData();
        formdata.append("file", file);
        const url = "api/upload";
        Api.postFile(url, formdata, (filePath) => {
            showFile(filePath, "chatBox");
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
        this.allFiles = fileList;
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


