var audioContext = null;
var mediaStreamSource = null;
var processor = null;
var isVoiceActive = false;
var voiceStream = null;

document.getElementById("voiceBtn").addEventListener("click", async function() {
    if (isVoiceActive) {
        stopVoiceChat();
    } else {
        await startVoiceChat();
    }
});

async function startVoiceChat() {
    try {
        voiceStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        
        audioContext = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 8000 });
        mediaStreamSource = audioContext.createMediaStreamSource(voiceStream);
        
        processor = audioContext.createScriptProcessor(1024, 1, 1);
        
        processor.onaudioprocess = function(e) {
            if (!isVoiceActive) return;
            var inputData = e.inputBuffer.getChannelData(0);
            var floatArray = new Array(inputData.length);
            for (var i = 0; i < inputData.length; i++) {
                floatArray[i] = inputData[i];
            }
            if (connection) {
                connection.invoke("SendAudioData", floatArray).catch(function() {});
            }
        };
        
        mediaStreamSource.connect(processor);
        processor.connect(audioContext.destination);
        
        isVoiceActive = true;
        document.getElementById("voiceBtn").style.background = "#ff4444";
        document.getElementById("voiceBtn").style.color = "white";
        document.getElementById("voiceBtn").style.borderColor = "#ff4444";
    } catch (err) {
        alert("Нет доступа к микрофону: " + err.message);
    }
}

function stopVoiceChat() {
    isVoiceActive = false;
    
    if (processor) {
        processor.disconnect();
        processor = null;
    }
    if (mediaStreamSource) {
        mediaStreamSource.disconnect();
        mediaStreamSource = null;
    }
    if (voiceStream) {
        voiceStream.getTracks().forEach(function(track) { track.stop(); });
        voiceStream = null;
    }
    if (audioContext) {
        audioContext.close();
        audioContext = null;
    }
    
    document.getElementById("voiceBtn").style.background = "transparent";
    document.getElementById("voiceBtn").style.color = "#666";
    document.getElementById("voiceBtn").style.borderColor = "#ccc";
}

var connection = null;

function setConnection(conn) {
    connection = conn;
    connection.on("ReceiveAudio", function(audioData, userName) {
        playAudio(audioData);
    });
}

var playContext = null;

function playAudio(floatArray) {
    try {
        if (!playContext) {
            playContext = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 8000 });
        }
        
        var buffer = playContext.createBuffer(1, floatArray.length, 8000);
        var channelData = buffer.getChannelData(0);
        for (var i = 0; i < floatArray.length; i++) {
            channelData[i] = floatArray[i];
        }
        
        var source = playContext.createBufferSource();
        source.buffer = buffer;
        source.connect(playContext.destination);
        source.start();
    } catch(e) {}
}