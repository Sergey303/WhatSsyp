function getFileType(filePath) {
    const name = filePath.toLowerCase();
    
    if (['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg', '.ico'].some(ext => name.endsWith(ext))) return '.tpl-img';
    if (['.mp4', '.webm', '.mov'].some(ext => name.endsWith(ext))) return '.tpl-video';
    if (['.mp3', '.wav', '.ogg', '.flac', '.aac', '.m4a'].some(ext => name.endsWith(ext))) return '.tpl-audio';
    if (['.pdf', '.txt', '.html', '.htm', '.json', '.xml'].some(ext => name.endsWith(ext))) return '.tpl-iframe';
    if (['.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.rtf', '.odt', '.ods', '.odp'].some(ext => name.endsWith(ext))) return '.tpl-g-iframe';
    
    return '.tpl-download-btn';
}

class FileUploadManager {
    constructor(options = {}) {
        this.maxFiles = options.maxFiles || 5;
        this.maxSizeBytes = options.maxSizeMB ? options.maxSizeMB * 1024 * 1024 : 10 * 1024 * 1024;

        this.fileInput = document.getElementById('fileInp');
        this.setupEventListeners();
    }

    setupEventListeners() {
        if (this.fileInput) {
            this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        }

        document.body.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });

        document.body.addEventListener('dragenter', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });

        document.body.addEventListener('drop', (e) => {
            e.preventDefault();
            e.stopPropagation();
            if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
                for (let i = 0; i < e.dataTransfer.files.length; i++) {
                    this.sendFile(e.dataTransfer.files[i]);
                }
            }
        });
    }

    handleFileSelect(event) {
        if (event.target.files && event.target.files.length > 0) {
            this.sendFile(event.target.files[0]);
        }
        this.fileInput.value = '';
    }
    
    sendFile(file) {
        const formdata = new FormData();
        formdata.append("file", file);
        Api.postFile("api/upload", formdata, (filePath) => {
            var fileData = {
                filePath: filePath,
                fileName: file.name
            };
            Chat.send("file", JSON.stringify(fileData));
        });
    }
}

const uploadManager = new FileUploadManager({
    maxFiles: 5,
    maxSizeMB: 10
});