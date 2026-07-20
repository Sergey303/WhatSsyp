function getFileType(filePath) {
    const name = filePath.toLowerCase();
    
    if (['.avif', '.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg', '.ico']
        .some(ext => name.endsWith(ext))) return '.tpl-img';

    if (['.mp4', '.webm', '.mov']
        .some(ext => name.endsWith(ext))) return '.tpl-video';
    
    if (['.mp3', '.wav', '.ogg', '.flac', '.aac', '.m4a']
        .some(ext => name.endsWith(ext))) return '.tpl-audio';

    if (['.pdf', '.txt', '.html', '.htm', '.json', '.xml']
        .some(ext => name.endsWith(ext))) return '.tpl-iframe';

    if (['.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.rtf', '.odt', '.ods', '.odp']
        .some(ext => name.endsWith(ext))) return '.tpl-g-iframe';
    
    return '.tpl-download-btn';
}
<<<<<<< HEAD
window.activateMedia = function activateMedia(fileUrl, text, name, date) {
=======
function activateMedia(fileUrl, text, name, date) {
>>>>>>> bb26018ab8eaa48e14e0074235cea2cf1ce57d05
    const type = getFileType(fileUrl);
    const fileName = fileUrl.trim().split("\\").at(-1);
    const messages = document.getElementById("chatBox");
    const fileMTemp = document.getElementById("file-template");
    const fileMsg = fileMTemp.content.cloneNode(true);
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
    
    // else if (type === '.tpl-iframe') {
    //     const iframe = fileMsg.querySelector(type);
    //     iframe.src = fileUrl;
    //     iframe.style.display = 'block';
    // } 

    // else if (type === '.tpl-g-iframe') {
    //     const giframe = fileMsg.querySelector(type);
    //     giframe.src = fileUrl;
    //     giframe.style.display = 'block';
    // } 
    else if (fileUrl) {
        const btn = fileMsg.querySelector('.tpl-download-btn');
        if (btn) 
        { 
            btn.href = fileUrl; 
            btn.style.display = 'block';
            btn.textContent = fileName;
        }
    }

    const nameBlc = fileMsg.querySelector(".name-block");
    const textBlc = fileMsg.querySelector(".text-block");
    const dateBlc = fileMsg.querySelector(".date-block");
    nameBlc.textContent = name;
    textBlc.textContent = text;
    dateBlc.textContent = date;
    messages.appendChild(fileMsg);
    scrollToBottom()
}

//formdata

class FileUploadManager {
    constructor(options = {}) {
        this.maxFiles = options.maxFiles || 5;
        this.maxSizeMB = options.maxSizeMB || 10;
        this.maxSizeBytes = +(options.maxSizeMB * 131072).toFixed(0);
        this.allowedTypes = options.allowedTypes || null;

        this.allFiles = [];
        this.files = [];
            
        this.fileInput = document.getElementById('fileInp');
        this.fileList = document.getElementById('fileList');
        this.stats = document.getElementById('stats');
        this.messages = document.getElementById('fileMessages');
        this.uploadBtn = document.getElementById('sendBtn');
        this.msgInput = document.getElementById('messageInp');
        this.uploadBtn.disabled = "false";

        this.setupEventListeners() ;
    }

    setupEventListeners() {
        this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        this.msgInput.addEventListener('change', () => this.changeSendBtn());
        this.uploadBtn.addEventListener('click', () => this.uploadFiles());
        
        const dropZone = document.getElementById('file-input-wrapper');
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

            this.handleFileDrop(e);
        });

        // dropZone.addEventListener('drop', (e) => {
        //     console.log(e);
        //     e.preventDefault();
        //     dropZone.style.borderColor = '#ccc';
        //     dropZone.style.background = '';
        //     this.handleFiles(e.dataTransfer.files);
        // });
    }
    changeSendBtn() {
        if (this.msgInput.value) {
            this.uploadBtn.disabled = false;
        } 
        else {
            this.uploadBtn.disabled = true;
        }
    }

    addFileToList(text) {
        if (text.length > 20) {
            text = text.slice(0, 16);
            text += '...';
        }
        const newItem = document.createElement('li');
        newItem.textContent = text;
        this.fileList.appendChild(newItem);
    }

    handleFileSelect(event) {
        console.log(event);
        const _nfs = this.handleFiles(event.target.files)
        for (const _f of _nfs) {
            this.files.push(_f);
            this.addFileToList(_f.name);
        }
        this.fileInput.value = '';
        this.uploadBtn.disabled = false;
    }

    handleFileDrop(event) {
        console.log(event);
        const _nfs = this.handleFiles(event.dataTransfer.files);
        for (const _f of _nfs) {
            this.files.push(_f);
        }
        this.uploadBtn.disabled = false;
    }
    
    sendFile(file) {
        if (!file) {
            sendMessage("");
        }
        const formdata = new FormData();
        formdata.append("file", file);
        Api.postFile("api/MLupload", formdata, (filePath) => {
            sendMessage(filePath);
        });
    }

    handleFiles(fileList) {
        const newFiles = Array.from(fileList);

        if (this.files.length + newFiles.length > this.maxFiles) {
            alert(`file amount exceeded (${this.files.length + newFiles.length
                }/${this.maxFiles}). New files were not uploaded!`);
            return;
        }
        
        const totalSize = this.getSize(this.files) 
            + this.getSize(newFiles);
        
        if (totalSize > this.maxSizeBytes) {
            const currentMB = (this.getSize(this.files) / (131072)).toFixed(2);
            const newMB = (this.getSize(newFiles) / (131072)).toFixed(2);
            alert(`files size exceeded (${currentMB + newMB}/${this.maxSizeMB
                }). New files were not uploaded!`);
            return;
        }
        
        for (const file of newFiles) {
            if (file.size > this.maxSizeBytes) {
                alert(`the ${file.name} esceeded max file size. New files were not uploaded!`)
                return;
            }
        }
        
        // if (this.allowedTypes) {
        //     for (const file of newFiles) {
        //         if (!this.isFileTypeAllowed(file)) {
        //             return;
        //         }
        //     }
        // }
        return newFiles;
    }
    
    // isFileTypeAllowed(file) {
    //     return this.allowedTypes.some(type => {
    //         if (type.startsWith('.')) {
    //             return file.name.toLowerCase().endsWith(type);
    //         }
    //         return file.type.startsWith(type);
    //     });
    // }
    
    getSize(files) {
        let size = 0;
        if (!files) {return 0;}
        for (const _f of files) {
            size = size + _f.size;
        }
        return size;
    }
    
    // getFileType(file) {
    //     const type = file.type;
    //     const name = file.name.toLowerCase();
        
    //     if (type.startsWith('image/')) return 'image';
    //     if (type.startsWith('video/')) return 'video';
    //     if (type.startsWith('audio/')) return 'audio';
    //     if (type === 'text/plain' || name.endsWith('.txt')) return 'text';
    //     if (type === 'application/json' || name.endsWith('.json')) return 'json';
    //     if (['.zip', '.rar', '.7z', '.tar', '.gz'].some(ext => name.endsWith(ext))) return 'archive';
    //     return 'other';
    // }
    
    async uploadFiles() {
        if (this.files.length === 0) {
<<<<<<< HEAD
            this.sendFile("");
=======
            this.sendFile(null);
>>>>>>> bb26018ab8eaa48e14e0074235cea2cf1ce57d05
        }
        const totalSize = this.getSize(this.files);
        if (totalSize > this.maxSizeBytes) {
            alert(`Total size esceeds the limit (${totalSize}/${this.maxSizeBytes})`)
            return;
        }
        this.uploadBtn.disabled = true;

        this.uploadBtn.textContent = 'Uploading...';
        
        try {
            console.log(this.files);
            for (const _f of this.files) {
                this.sendFile(_f);
            }
        } catch (error) {
            console.error(error);
        } finally {
            this.fileList.replaceChildren(); 
            this.files = [];
            this.uploadBtn.disabled = true;
            this.uploadBtn.textContent = 'Send';
        }
    }
    
    // readFileAsDataURL(file) {
    //     return new Promise((resolve, reject) => {
    //         const reader = new FileReader();
    //         reader.onload = (e) => resolve(e.target.result);
    //         reader.onerror = reject;
    //         reader.readAsDataURL(file);
    //     });
    // }
}

// Initialize with custom limits
const uploadManager = new FileUploadManager({
    maxFiles: 5,
    maxSizeMB: 10,
    allowedTypes: null // Set to ['image/', '.pdf'] to limit types
});


