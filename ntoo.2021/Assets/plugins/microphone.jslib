var LibraryMicrophone =
{
    $microphoneWorker:
    {
        initResult: "not initialized",
        capture: false,
        buffers: [],
    },

    MicrophoneWebGL_Init: function(bufferSize, numberOfChannels)
    {
        var mw = microphoneWorker;

        navigator.getUserMedia = (navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia);

        if (!navigator.getUserMedia)
        {
            mw.initResult = "getUserMedia not supported by browser";
            return;
        }

        mw.initResult = "pending";

        navigator.getUserMedia(

            { audio: true }, 

            function(stream)
            {
                mw.stream = stream; // we need to keep this alive or the gc kills it on Firefox

                var audioContext = new window.AudioContext;
                
                var scriptNode = audioContext.createScriptProcessor(bufferSize, numberOfChannels, numberOfChannels);
                scriptNode.onaudioprocess = function(e)
                {
                    if (!mw.capture) return;
                    for (var channel = 0; channel < e.inputBuffer.numberOfChannels; ++channel)
                    {
                        mw.buffers.push(e.inputBuffer.getChannelData(channel));
                    }
                };

                var input = audioContext.createMediaStreamSource(stream);
                input.connect(scriptNode);

                var sink = audioContext.createMediaStreamDestination();
                scriptNode.connect(sink);
                
                mw.initResult = "ready";
            },

            function(e)
            {
                mw.initResult = "getUserMedia error: " + e;
            }
        );
    },

    MicrophoneWebGL_PollInit: function(resultPtr, resultMaxLength)
    {
        var str = microphoneWorker.initResult.slice(0, Math.max(0, resultMaxLength/2 - 1));
        stringToUTF16(str, HEAPU32[resultPtr>>2]);
    },

    MicrophoneWebGL_Start: function()
    {
        microphoneWorker.capture = true;
    },

    MicrophoneWebGL_Stop: function()
    {
        microphoneWorker.capture = false;
    },

    MicrophoneWebGL_GetNumBuffers: function()
    {
        return microphoneWorker.buffers.length;
    },

    MicrophoneWebGL_GetBuffer: function(bufferPtr)
    {
        if (microphoneWorker.buffers.length == 0)
        {
            return false;
        }
        HEAPF32.set(microphoneWorker.buffers.shift(), HEAPU32[bufferPtr>>2]>>2);
        return true;
    },
};

autoAddDeps(LibraryMicrophone, '$microphoneWorker')
mergeInto(LibraryManager.library, LibraryMicrophone);

