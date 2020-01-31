(function () {
  'use strict';

  function XRData() {
    this.leftProjectionMatrix = mat4.create();
    this.rightProjectionMatrix = mat4.create();
    this.leftViewMatrix = mat4.create();
    this.rightViewMatrix = mat4.create();
    this.sitStandMatrix = mat4.create();
    this.gamepads = [];
    this.xrData = null;
  }

  function XRManager() {
    this.enterVRButton = document.getElementById('entervr');
    this.gameContainer = document.getElementById('game');
    // Rendering resolution scale
    this.scaleResolution = 1;
    // Unity GameObject name which we will SendMessage to
    this.unityObjectName = 'WebXRCameraSet';

    this.xrSession = null;
    this.xrData = new XRData();
    this.canvas = null;
    this.gameInstance = null;
    this.polyfill = null;
    this.toggleVRKeyName = '';
    this.wasPresenting = false;
    this.init();
  }

  XRManager.prototype.init = async function () {
    if (window.WebXRPolyfill) {
      this.polyfill = new WebXRPolyfill();
    }

    this.attachEventListeners();

    await this.getVRDisplay();
    if (this.vrDisplay) {
      // Unity drives its rendering from the window `rAF`. We reassign to use `VRDisplay`'s `rAF` when presenting
      // such that Unity renders at the VR display's proper framerate.
      window.requestAnimationFrame = this.requestAnimationFrame.bind(this);
    }
  }

  XRManager.prototype.requestAnimationFrame = function(cb) {
    if (this.xrSession) {
      return this.xrSession.requestAnimationFrame(cb);
    }
  }

  XRManager.prototype.attachEventListeners = function () {
    var onResize = this.resize.bind(this);
    var onToggleVr = this.toggleVr.bind(this);
    var onKeyUp = this.keyUp.bind(this);
    var onActivate = this.activate.bind(this);
    var onUnityLoaded = this.unityLoaded.bind(this);
    var onUnityMessage = this.unityMessage.bind(this);

    window.addEventListener('vrdisplayactivate', onActivate);
    window.addEventListener('resize', onResize, true);
    window.addEventListener('keyup', onKeyUp, false);

    // dispatched by index.html
    document.addEventListener('UnityLoaded', onUnityLoaded, false);
    document.addEventListener('Unity', onUnityMessage, false);

    this.enterVRButton.addEventListener('click', onToggleVr, false);
  }

  XRManager.prototype.resize = function () {
    if (!this.canvas) return;

    if (this.vrDisplay && this.vrDisplay.isPresenting) {
      var leftEye = this.vrDisplay.getEyeParameters('left');
      var rightEye = this.vrDisplay.getEyeParameters('right');
      var renderWidth = Math.max(leftEye.renderWidth, rightEye.renderWidth) * 2 * this.scaleResolution;
      var renderHeight = Math.max(leftEye.renderHeight, rightEye.renderHeight) * this.scaleResolution;
      this.canvas.width = renderWidth;
      this.canvas.height = renderHeight;

      // scale game container so we get a proper sized mirror of VR content to desktop.
      if (this.vrDisplay.capabilities.hasExternalDisplay) {
        var scaleX = window.innerWidth / renderWidth;
        var scaleY = window.innerHeight / renderHeight;
        this.gameContainer.setAttribute('style', `transform: scale(${scaleX}, ${scaleY}); transform-origin: top left;`);
      }
    } else {
      this.canvas.width = window.innerWidth;
      this.canvas.height = window.innerHeight;
      this.gameContainer.style.transform = '';
    }
  }

  XRManager.prototype.requestPresent = function (canvas) {
    if (!this.vrDisplay) return;

    this.vrDisplay.requestPresent([{ source: canvas }]).then(function () {
      console.log('Entered VR');
    }).catch(function (err) {
      console.error('Unable to enter VR mode: ', err);
    });
  }

  XRManager.prototype.exitPresent = function () {
    if (!this.vrDisplay && !this.vrDisplay.isPresenting) {
      console.warn('No VR display to exit VR mode');
      return;
    }

    return this.vrDisplay.exitPresent().then(function () {
      console.log('Exited VR');
    }).catch(function (err) {
      console.error('Unable to exit VR mode:', err);
    });
  }

  XRManager.prototype.toggleVr = function () {
    if (this.vrDisplay && this.vrDisplay.isPresenting && this.gameInstance) {
      this.exitPresent();
    } else {
      this.requestPresent(this.canvas);
    }
  }

  XRManager.prototype.keyUp = function (evt) {
    if (this.toggleVRKeyName && this.toggleVRKeyName === evt.key) {
      this.toggleVr();
    }
  }

  XRManager.prototype.activate = async function (evt) {
    if (!evt.display) {
      console.error('No `display` property found on event');
      return;
    }
    if (evt.reason && evt.reason !== 'navigation') {
      console.error("Unexpected `reason` (expected to be 'navigation')")
      return;
    }
    if (!evt.display.capabilities || !evt.display.capabilities.canPresent) {
      console.error('VR display is not capable of presenting');
      return;
    }
    this.setVRDisplay(evt.display);
    this.setGameInstance(await this.unityProgressStart);
    this.requestPresent(this.canvas);
  }

  XRManager.prototype.setVRDisplay = function(display) {
    this.vrDisplay = display;

    if (this.vrDisplay.capabilities.canPresent) {
      this.enterVRButton.dataset.enabled = true;
    }
  }

  XRManager.prototype.getVRDisplay = function () {
    if (this.vrDisplay) {
      return Promise.resolve(this.vrDisplay);
    }

    return navigator.getVRDisplays().then(function (displays) {
      if (!displays.length) {
        return null;
      }
      this.setVRDisplay(displays[displays.length - 1]);
      return Promise.resolve(this.vrDisplay);
    }.bind(this));
  }

  XRManager.prototype.setGameInstance = function (gameInstance) {
    if (!this.gameInstance) {
      this.gameInstance = gameInstance;
      this.canvas = this.gameInstance.Module.canvas;
    }
  }

  XRManager.prototype.unityProgressStart = new Promise(function (resolve) {
    // dispatched by index.html
    document.addEventListener('UnityProgressStart', function (evt) {
      resolve(window.gameInstance);
    }, false);
  });

  XRManager.prototype.unityLoaded = async function () {
    MozillaResearch.telemetry.performance.measure('LoadingTime', 'LoadingStart');
    document.body.dataset.unityLoaded = 'true';

    // Send browser capabilities to Unity.
    var canPresent = false;
    var hasPosition = false;
    var hasExternalDisplay = false;

    if (this.vrDisplay) {
      var capabilities = this.vrDisplay.capabilities
      canPresent = capabilities.canPresent;
      hasPosition = capabilities.hasPosition;
      hasExternalDisplay = capabilities.hasExternalDisplay;
    }

    this.setGameInstance(await this.unityProgressStart);
    this.resize();

    this.gameInstance.SendMessage(
      this.unityObjectName, 'OnVRCapabilities',
      JSON.stringify({
        canPresent: canPresent,
        hasPosition: hasPosition,
        hasExternalDisplay: hasExternalDisplay
      })
    );
  }

  XRManager.prototype.getGamepadAxes = function(gamepad) {
    var axes = [];
    for (var i = 0; i < gamepad.axes.length; i++) {
      axes.push(gamepad.axes[i]);
    }
    return axes;
  }

  XRManager.prototype.getGamepadButtons = function(gamepad) {
    var buttons = [];
    for (var i = 0; i < gamepad.buttons.length; i++) {
      buttons.push({
        pressed: gamepad.buttons[i].pressed,
        touched: gamepad.buttons[i].touched,
        value: gamepad.buttons[i].value
      });
    }
    return buttons;
  }

  XRManager.prototype.getGamepads = function(gamepads) {
    var vrGamepads = []
    for (var i = 0; i < gamepads.length; ++i) {
      var gamepad = gamepads[i];

      if (gamepad) {
        if (gamepad.pose || gamepad.displayId) {
          var position = gamepad.pose && gamepad.pose.position;
          var orientation = gamepad.pose && gamepad.pose.orientation;
          var linearAcceleration = gamepad.pose && gamepad.pose.linearAcceleration;
          var linearVelocity = gamepad.pose && gamepad.pose.linearVelocity;

          position = position ? this.GLVec3ToUnity(position) : [0, 0, 0];
          orientation = orientation ? this.GLQuaternionToUnity(orientation) : [0, 0, 0, 1];
          linearAcceleration = linearAcceleration ? this.GLVec3ToUnity(linearAcceleration) : [0, 0, 0];
          linearVelocity = linearVelocity ? this.GLVec3ToUnity(linearVelocity) : [0, 0, 0];

          vrGamepads.push({
            id: gamepad.id,
            index: gamepad.index,
            hand: gamepad.hand,
            buttons: this.getGamepadButtons(gamepad),
            axes: this.getGamepadAxes(gamepad),
            hasOrientation: gamepad.pose.hasOrientation,
            hasPosition: gamepad.pose.hasPosition,
            orientation: Array.from(orientation),
            position: Array.from(position),
            linearAcceleration: Array.from(linearAcceleration),
            linearVelocity: Array.from(linearVelocity)
          });
        }
      }
    }
    return vrGamepads;
  }

  // Convert WebGL to Unity compatible Vector3
  XRManager.prototype.GLVec3ToUnity = function(v) {
    v[2] *= -1;
    return v;
  }

  // Convert WebGL to Unity compatible Quaternion
  XRManager.prototype.GLQuaternionToUnity = function(q) {
    q[0] *= -1;
    q[1] *= -1;
    return q;
  }

  // Convert WebGL to Unity Projection Matrix4
  XRManager.prototype.GLProjectionToUnity = function(m) {
    var out = mat4.create();
    mat4.copy(out, m)
    mat4.transpose(out, out);
    return out;
  }

  // Convert WebGL to Unity View Matrix4
  XRManager.prototype.GLViewToUnity = function(m) {
    var out = mat4.create();
    mat4.copy(out, m);
    mat4.transpose(out, out);
    out[2] *= -1;
    out[6] *= -1;
    out[10] *= -1;
    out[14] *= -1;
    return out;
  }

  XRManager.prototype.animate = function () {
    if (!this.vrDisplay) {
      return;
    }

    if (this.vrDisplay.isPresenting && !this.wasPresenting) {
      this.gameInstance.SendMessage(this.unityObjectName, 'OnStartVR');
      this.wasPresenting = true;
      this.resize();
    }

    if (!this.vrDisplay.isPresenting && this.wasPresenting) {
      this.gameInstance.SendMessage(this.unityObjectName, 'OnEndVR');
      this.wasPresenting = false;
      this.resize();
    }

    if (!this.vrDisplay.isPresenting) {
      return;
    }

    var vrData = this.vrData;
    vrData.frameData = new VRFrameData();
    this.vrDisplay.getFrameData(vrData.frameData);

    vrData.leftProjectionMatrix = this.GLProjectionToUnity(vrData.frameData.leftProjectionMatrix);
    vrData.rightProjectionMatrix = this.GLProjectionToUnity(vrData.frameData.rightProjectionMatrix);
    vrData.rightViewMatrix = this.GLViewToUnity(vrData.frameData.rightViewMatrix);
    vrData.leftViewMatrix = this.GLViewToUnity(vrData.frameData.leftViewMatrix);

    // Sit Stand transform
    if (this.vrDisplay.stageParameters) {
      mat4.copy(vrData.sitStandMatrix, this.vrDisplay.stageParameters.sittingToStandingTransform);
    }
    mat4.transpose(vrData.sitStandMatrix, vrData.sitStandMatrix);

    // Gamepads
    vrData.gamepads = this.getGamepads(navigator.getGamepads());

    // Dispatch event with headset data to be handled in webxr.jslib
    document.dispatchEvent(new CustomEvent('VRData', { detail: {
      leftProjectionMatrix: vrData.leftProjectionMatrix,
      rightProjectionMatrix: vrData.rightProjectionMatrix,
      leftViewMatrix: vrData.leftViewMatrix,
      rightViewMatrix: vrData.rightViewMatrix,
      sitStandMatrix: vrData.sitStandMatrix
    }}));

    gameInstance.SendMessage('WebVRCameraSet', 'OnWebVRData', JSON.stringify({
      controllers: vrData.gamepads
    }));

    this.vrDisplay.submitFrame();
  }

  XRManager.prototype.unityMessage = function (msg) {
      var animate = this.animate.bind(this);

      if (typeof msg.detail === 'string') {
        // Wait for Unity to render the frame; then submit the frame to the VR display.
        if (msg.detail === 'PostRender') {
          if (this.vrDisplay && this.vrDisplay.isPresenting) {
            this.vrDisplay.requestAnimationFrame(animate);
          }
        }

        // Assign VR toggle key from Unity on WebXRManager component.
        if (msg.detail.indexOf('ConfigureToggleVRKeyName') === 0) {
          this.toggleVRKeyName = msg.detail.split(':')[1];
        }
      }

      // Handle UI dialogue
      if (msg.detail.type === 'displayElementId') {
        var el = document.getElementById(msg.detail.id);
        this.displayElement(el);
      }
  }

  // Show instruction dialogue for non-VR enabled browsers.
  XRManager.prototype.displayElement = function (el) {
    if (el.dataset.enabled) {
      return;
    }
    var confirmButton = el.querySelector('button');
    el.dataset.enabled = true;

    function onConfirm () {
      el.dataset.enabled = false;
      confirmButton.removeEventListener('click', onConfirm);
    }
    confirmButton.addEventListener('click', onConfirm);
  }

  function initWebXRManager () {
    var xrManager = window.xrManager = new XRManager();
    return xrManager;
  }

  function init() {
    if (!navigator.xr) {
      var script = document.createElement('script');
      script.src = 'https://cdn.jsdelivr.net/npm/webxr-polyfill@latest/build/webxr-polyfill.js';
      document.getElementsByTagName('head')[0].appendChild(script);

      script.addEventListener('load', function () {
        initWebXRManager();
      });

      script.addEventListener('error', function (err) {
        console.warn('Could not load the WebXR Polyfill script:', err);
      });
    }

    initWebXRManager();
  }

  init();
})();