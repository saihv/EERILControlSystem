using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using PvNET;

namespace EERIL.ControlSystem.Avt
{
    public class Camera : ICamera
    {
        private tCameraInfo cameraInfo;
        private uint? camera;
        private GCHandle[] frameBufferHandles;
        private GCHandle[] framePoolHandles;
        private tFrame[] frames;
        private const int FRAME_POOL_SIZE = 10;
        private const int FULL_BIT_DEPTH = 12; // ADC max res. = 12 for Prosilica GC1380
        private const UInt32 MAX_PACKET_SIZE = 16456;
        private readonly Dictionary<IntPtr, byte[]> buffers = new Dictionary<IntPtr, byte[]>();
        private readonly tFrameCallback callback;
        private readonly Timer heartbeatTimer;
        private readonly byte[] heartbeat = new byte[] { 0xA6, 0x0D };

        public event FrameReadyHandler FrameReady;

        protected void OnFrameReady(IntPtr framePointer)
        {
            if (FrameReady != null)
            {
                tFrame tFrame = (tFrame)Marshal.PtrToStructure(framePointer, typeof(tFrame));
                Frame frame = new Frame(this, framePointer, tFrame, buffers[framePointer]);
                FrameReady(this, frame);
            }
        }

        internal void ReleaseFrame(IntPtr framePointer)
        {
            if (!this.camera.HasValue)
            {
                throw new PvException(tErr.eErrUnavailable);
            }
            tErr error = Pv.CaptureQueueFrame(this.camera.Value, framePointer, this.callback);
            if (error != tErr.eErrSuccess)
            {
                throw new PvException(error); // TODO: throws exception here
            }
        }

        public Camera(tCameraInfo cameraInfo)
        {
            this.heartbeatTimer = new Timer(Heartbeat, null, 100, 100);
            this.cameraInfo = cameraInfo;
            this.callback = new tFrameCallback(OnFrameReady);
        }

        public void Heartbeat(object state)
        {
            this.WriteBytesToSerial(heartbeat);
        }


        public uint UniqueId
        {
            get { return cameraInfo.UniqueId; }
        }

        public string SerialString
        {
            get { return cameraInfo.SerialString; }
        }

        public uint PartNumber
        {
            get { return cameraInfo.PartNumber; }
        }

        public uint PartVersion
        {
            get { return cameraInfo.PartVersion; }
        }

        public uint PermittedAccess
        {
            get { return cameraInfo.PermittedAccess; }
        }

        public uint InterfaceId
        {
            get { return cameraInfo.InterfaceId; }
        }

        public tInterface InterfaceType
        {
            get { return (tInterface)cameraInfo.InterfaceType; }
        }

        public string DisplayName
        {
            get { return cameraInfo.DisplayName; }
        }

        public uint Reference
        {
            get { return camera.Value; }
        }

        public float Temperature
        {
            get
            {
                float value = 0;
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrFloat32Get(camera.Value, "DeviceTemperatureMainboard", ref value);
                return value;
            }
        }

        public uint ImageHeight
        {
            get
            {
                uint value = 0;
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrUint32Get(camera.Value, "Height", ref value);
                return value;
            }
            set
            {
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrUint32Set(camera.Value, "Height", value);
            }
        }

        public uint ImageWidth
        {
            get
            {
                uint value = 0;
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrUint32Get(camera.Value, "Width", ref value);
                return value;
            }
            set
            {
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrUint32Set(camera.Value, "Width", value);
            }
        }

        public uint ImageDepth
        {
            get
            {
                uint value = 0;
                switch (ImageFormat)
                {
                    case tImageFormat.eFmtMono8:
                    case tImageFormat.eFmtBayer8:
                    case tImageFormat.eFmtRgb24:
                    case tImageFormat.eFmtBgr24:
                    case tImageFormat.eFmtYuv411:
                    case tImageFormat.eFmtYuv422:
                    case tImageFormat.eFmtYuv444:
                    case tImageFormat.eFmtRgba32:
                    case tImageFormat.eFmtBgra32:
                        value = 8;
                        break;

                    case tImageFormat.eFmtMono12Packed:
                    case tImageFormat.eFmtBayer12Packed:
                        value = 12;
                        break;

                    case tImageFormat.eFmtMono16:
                    case tImageFormat.eFmtBayer16:
                    case tImageFormat.eFmtRgb48:
                        value = FULL_BIT_DEPTH; // depends on hardware
                        break;

                    default:
                        value = 0;
                        break;
                }
                return value;
            }
        }

        public float BytesPerPixel
        {
            get
            {
                float value = 0;
                switch (ImageFormat)
                {
                    case tImageFormat.eFmtMono8:
                    case tImageFormat.eFmtBayer8:
                        value = 1.0F;
                        break;

                    case tImageFormat.eFmtYuv411:
                    case tImageFormat.eFmtMono12Packed:
                    case tImageFormat.eFmtBayer12Packed:
                        value = 1.5F;
                        break;

                    case tImageFormat.eFmtMono16:
                    case tImageFormat.eFmtBayer16:
                    case tImageFormat.eFmtYuv422:
                        value = 2.0F;
                        break;

                    case tImageFormat.eFmtRgb24:
                    case tImageFormat.eFmtBgr24:
                    case tImageFormat.eFmtYuv444:
                        value = 3.0F;
                        break;

                    case tImageFormat.eFmtRgba32:
                    case tImageFormat.eFmtBgra32:
                        value = 4.0F;
                        break;

                    case tImageFormat.eFmtRgb48:
                        value = 6.0F;
                        break;

                    default:
                        value = 0.0F;
                        break;
                }
                return value;
            }
        }

        public tImageFormat ImageFormat
        {
            get
            {
                tImageFormat value = 0;
                StringBuilder buffer = new StringBuilder(16);
                UInt32 read = 0;
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrEnumGet(camera.Value, "PixelFormat", buffer, 16, ref read);
                value = (tImageFormat)Enum.Parse(typeof(tImageFormat), buffer.ToString().Substring(4, buffer.Length - 4), true); // parse without "eFmt" in positions 0-3
                return value;
            }
            set
            {
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrUint32Set(camera.Value, "PixelFormat", (uint)value);
            }
        }

        public ColorTransformation ColorTransformation
        {
            get
            {
                ColorTransformation value = new ColorTransformation();
                StringBuilder buffer = new StringBuilder(16);
                UInt32 read = 0;
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                Pv.AttrEnumGet(camera.Value, "ColorTransformationMode", buffer, 16, ref read);
                value.Mode = (ColorTransformationMode)Enum.Parse(typeof(ColorTransformationMode), buffer.ToString(), true);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueBB", ref value.ValueBB);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueBG", ref value.ValueBG);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueBR", ref value.ValueBR);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueGB", ref value.ValueGB);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueGG", ref value.ValueGG);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueGR", ref value.ValueGR);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueRB", ref value.ValueRB);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueRG", ref value.ValueRG);
                Pv.AttrFloat32Get(camera.Value, "ColorTransformationValueRR", ref value.ValueRR);
                return value;
            }
            set
            {
                if (!camera.HasValue)
                {
                    throw new PvException(tErr.eErrUnavailable);
                }
                if (value.ValueBB != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueBB", value.ValueBB); }
                if (value.ValueBG != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueBG", value.ValueBG); }
                if (value.ValueBR != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueBR", value.ValueBR); }
                if (value.ValueGB != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueGB", value.ValueGB); }
                if (value.ValueGG != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueGG", value.ValueGG); }
                if (value.ValueGR != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueGR", value.ValueGR); }
                if (value.ValueRB != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueRB", value.ValueRB); }
                if (value.ValueRG != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueRG", value.ValueRG); }
                if (value.ValueRR != -1) { Pv.AttrFloat32Set(camera.Value, "ColorTransformationValueRR", value.ValueRR); }
            }
        }

        public void BeginCapture()
        {
            tErr error;
            if (!camera.HasValue)
            {
                error = tErr.eErrUnavailable;
                throw new PvException(error);
            }
            //error = Pv.AttrEnumSet(this.camera.Value, "PixelFormat", "Rgb24");
            //if (error != tErr.eErrSuccess)
            //    goto error;
            error = Pv.CaptureStart(this.camera.Value);
            if (error != tErr.eErrSuccess)
                goto error;

            frameBufferHandles = new GCHandle[FRAME_POOL_SIZE];
            framePoolHandles = new GCHandle[FRAME_POOL_SIZE];
            frames = new tFrame[FRAME_POOL_SIZE];

            uint bufferSize = 0;
            error = Pv.AttrUint32Get(this.camera.Value, "TotalBytesPerFrame", ref bufferSize);
            if (error != tErr.eErrSuccess)
                goto error;
            byte[] buffer;
            GCHandle bufferHandle, frameHandle;
            tFrame frame;
            IntPtr framePointer;
            for (int count = FRAME_POOL_SIZE - 1; count >= 0; count--)
            {
                buffer = new byte[bufferSize];
                bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                frameBufferHandles[count] = bufferHandle;
                frame = new tFrame
                    {
                        ImageBuffer = bufferHandle.AddrOfPinnedObject(),
                        ImageBufferSize = bufferSize,
                        AncillaryBufferSize = 0
                    };
                frames[count] = frame;
                frameHandle = GCHandle.Alloc(frame, GCHandleType.Pinned);
                framePoolHandles[count] = frameHandle;
                framePointer = frameHandle.AddrOfPinnedObject();
                buffers.Add(framePointer, buffer);
                error = Pv.CaptureQueueFrame(this.camera.Value, framePointer, this.callback);
                if (error != tErr.eErrSuccess)
                    goto error;
            }
            error = Pv.AttrFloat32Set(this.camera.Value, "FrameRate", 15);
            if (error != tErr.eErrSuccess)
                goto error;
            error = Pv.AttrEnumSet(this.camera.Value, "FrameStartTriggerMode", "FixedRate");
            if (error != tErr.eErrSuccess)
                goto error;
            error = Pv.AttrEnumSet(this.camera.Value, "AcquisitionMode", "Continuous");
            if (error != tErr.eErrSuccess)
                goto error;
            error = Pv.CommandRun(this.camera.Value, "AcquisitionStart");
            if (error != tErr.eErrSuccess)
                goto error;
            return;
        error:
            EndCapture();

            throw new PvException(error);
        }

        public void EndCapture()
        {
            if (!camera.HasValue)
            {
                throw new PvException(tErr.eErrUnavailable);
            }
            Pv.CaptureQueueClear(this.camera.Value);
            foreach (GCHandle handle in framePoolHandles)
            {
                handle.Free();
            }
            foreach (GCHandle handle in frameBufferHandles)
            {
                handle.Free();
            }
            frames = null;
            Pv.CommandRun(this.camera.Value, "AcquisitionStop");
            Pv.CaptureEnd(this.camera.Value);
        }

        public bool WriteBytesToSerial(byte[] buffer)
        {
            heartbeatTimer.Change(100, 100);
            return camera.HasValue && CameraSerial.WriteBytesToSerialIo(camera.Value, buffer, Convert.ToUInt32(buffer.LongLength));
        }

        public bool ReadBytesFromSerial(byte[] buffer, ref uint recieved)
        {
            return camera.HasValue && CameraSerial.ReadBytesFromSerialIO(camera.Value, buffer, Convert.ToUInt32(buffer.LongLength), ref recieved);
        }

        public void Open()
        {
            if (!camera.HasValue)
            {
                uint cameraId;
                Pv.CameraClose(cameraInfo.UniqueId);
                tErr err = Pv.CameraOpen(cameraInfo.UniqueId, tAccessFlags.eAccessMaster, out cameraId);
                if (err != tErr.eErrSuccess)
                {
                    throw new PvException(err);
                }
                camera = cameraId;
                CameraSerial.Setup(cameraId);
                WriteBytesToSerial(new byte[] { 0x6F, 0x0D });
            }
        }

        public void AdjustPacketSize()
        {
            if (!camera.HasValue)
            {
                throw new PvException(tErr.eErrUnavailable);
            }
            Pv.CaptureAdjustPacketSize(cameraInfo.UniqueId, MAX_PACKET_SIZE);
        }

        public void Close()
        {
            if (camera.HasValue)
            {
                tErr err = Pv.CameraClose(camera.Value);
                camera = null;
                if (err != tErr.eErrSuccess)
                {
                    throw new PvException(err);
                }
            }
        }
    };
}