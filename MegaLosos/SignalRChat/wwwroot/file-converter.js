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
            this.updateUI();
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
        this.handleFiles(event.target.files);
        this.fileInput.value = ''; // Reset input
    }
    
    handleFiles(fileList) {
        const newFiles = Array.from(fileList);
        
        // Check file count
        if (this.files.length + newFiles.length > this.maxFiles) {
            this.showMessage(`You can only upload a maximum of ${this.maxFiles} files.`, 'error');
            return;
        }
        
        // Check total size
        const totalSize = this.getTotalSize() + newFiles.reduce((sum, f) => sum + f.size, 0);
        if (totalSize > this.maxSizeBytes) {
            const currentMB = (this.getTotalSize() / (1024 * 1024)).toFixed(2);
            const newMB = (newFiles.reduce((sum, f) => sum + f.size, 0) / (1024 * 1024)).toFixed(2);
            this.showMessage(
                `Total size would exceed ${this.maxSizeBytes / (1024 * 1024)}MB. ` +
                `Current: ${currentMB}MB, Adding: ${newMB}MB`,
                'error'
            );
            return;
        }
        
        // Check individual file size
        for (const file of newFiles) {
            if (file.size > this.maxSizeBytes) {
                this.showMessage(`File "${file.name}" exceeds the individual file limit of ${this.maxSizeBytes / (1024 * 1024)}MB.`, 'error');
                return;
            }
        }
        
        // Check file types if specified
        if (this.allowedTypes) {
            for (const file of newFiles) {
                if (!this.isFileTypeAllowed(file)) {
                    this.showMessage(`File "${file.name}" type is not allowed. Allowed: ${this.allowedTypes.join(', ')}`, 'error');
                    return;
                }
            }
        }
        
        // Add files
        this.files.push(...newFiles);
        this.showMessage(`${newFiles.length} file(s) added successfully!`, 'success');
        this.updateUI();
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
    
    removeFile(index) {
        this.files.splice(index, 1);
        this.updateUI();
        this.showMessage('File removed.', 'success');
    }
    
    updateUI() {
        const totalSizeMB = (this.getTotalSize() / (1024 * 1024)).toFixed(2);
        const maxSizeMB = (this.maxSizeBytes / (1024 * 1024));
        const remainingMB = (maxSizeMB - parseFloat(totalSizeMB)).toFixed(2);
        
        // Update stats
        this.stats.innerHTML = `
            <strong>Files:</strong> ${this.files.length}/${this.maxFiles} &nbsp;|&nbsp;
            <strong>Total Size:</strong> ${totalSizeMB}MB / ${maxSizeMB}MB &nbsp;|&nbsp;
            <strong>Remaining:</strong> ${remainingMB}MB
        `;
        
        // Update file list
        this.fileList.innerHTML = '';
        
        if (this.files.length === 0) {
            this.fileList.innerHTML = '<p style="color: #888;">No files selected</p>';
            this.uploadBtn.disabled = true;
            return;
        }
        
        this.files.forEach((file, index) => {
            const div = document.createElement('div');
            div.className = 'file-item';
            
            const fileType = this.getFileType(file);
            let previewHTML = '';
            
            if (fileType === 'image') {
                const reader = new FileReader();
                reader.onload = (e) => {
                    const img = div.querySelector('.file-preview');
                    if (img) img.src = e.target.result;
                };
                reader.readAsDataURL(file);
                previewHTML = `<img class="file-preview" alt="${file.name}">`;
            } else if (fileType === 'video') {
                previewHTML = `<span style="font-size: 2em;">🎬</span>`;
            } else if (fileType === 'audio') {
                previewHTML = `<span style="font-size: 2em;">🎵</span>`;
            } else if (fileType === 'text') {
                previewHTML = `<span style="font-size: 2em;">📄</span>`;
            } else if (fileType === 'archive') {
                previewHTML = `<span style="font-size: 2em;">📦</span>`;
            } else {
                previewHTML = `<span style="font-size: 2em;">📎</span>`;
            }
            
            const size = this.formatFileSize(file.size);
            
            div.innerHTML = `
                ${previewHTML}
                <div class="file-info">
                    <div><strong>${file.name}</strong></div>
                    <div class="file-size">${size} - ${file.type || 'Unknown'}</div>
                </div>
                <button class="remove-btn" data-index="${index}">Remove</button>
            `;
            
            this.fileList.appendChild(div);
        });
        
        // Add event listeners for remove buttons
        this.fileList.querySelectorAll('.remove-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const index = parseInt(e.target.dataset.index);
                this.removeFile(index);
            });
        });
        
        this.uploadBtn.disabled = false;
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
    
    showMessage(text, type = 'info') {
        const div = document.createElement('div');
        div.className = type === 'error' ? 'error-message' : 'success-message';
        div.textContent = text;
        this.messages.appendChild(div);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (div.parentNode) {
                div.remove();
            }
        }, 5000);
    }
    
    async uploadFiles() {
        if (this.files.length === 0) {
            this.showMessage('No files to upload!', 'error');
            return;
        }
        
        const totalSize = this.getTotalSize();
        if (totalSize > this.maxSizeBytes) {
            this.showMessage(`Total size (${(totalSize / (1024 * 1024)).toFixed(2)}MB) exceeds limit (${this.maxSizeBytes / (1024 * 1024)}MB)`, 'error');
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
                this.showMessage(`Successfully uploaded ${this.files.length} files!`, 'success');
                this.files = [];
                this.updateUI();
            } else {
                this.showMessage(`Upload failed: ${result.error}`, 'error');
            }
        } catch (error) {
            this.showMessage(`Upload error: ${error.message}`, 'error');
        } finally {
            this.uploadBtn.disabled = false;
            this.uploadBtn.textContent = 'Upload Files';
        }
    }
    
    // Alternative: Send as Base64
    async uploadAsBase64() {
        const filesData = [];
        
        for (const file of this.files) {
            const base64 = await this.readFileAsDataURL(file);
            filesData.push({
                name: file.name,
                type: file.type,
                size: file.size,
                data: base64
            });
        }
        
        const response = await fetch('/upload-base64-multiple', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ files: filesData })
        });
        
        return await response.json();
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
