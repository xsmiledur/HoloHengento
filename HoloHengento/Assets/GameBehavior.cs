using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

using System;

using System.Collections;
using System.IO;


//#if UNITY_UWP
//using System.IO;
//using System.Threading.Tasks;
//using Windows.Networking;
//using Windows.Networking.Sockets;
//#endif

//public class TcpNetworkClientManager
//{
//#if UNITY_UWP
//    private StreamWriter writer = null;
//    private Stream st;
//    private int len = 0;
//#endif

//    public TcpNetworkClientManager(string IP, int port)
//    {
//#if UNITY_UWP
//        Task.Run(async () => {
//            StreamSocket socket = new StreamSocket();
//            await socket.ConnectAsync(new HostName(IP), port.ToString());

//            st = socket.OutputStream.AsStreamForWrite();
//            writer = new StreamWriter(socket.OutputStream.AsStreamForWrite());

//            // サーバーへの接続テスト．返答待ち
//            StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
//            try
//            {
//                string data = await reader.ReadToEndAsync();
//            }
//            catch (Exception) { }
//            writer = null;
//        });
//#endif
//    }

//    public void SendData(byte[] data)
//    {
//#if UNITY_UWP
//        //if (writer != null) Task.Run(async () =>
//        //{
//        st.Write(data, 0, data.Length);
//        len = data.Length;
//            //await writer.WriteAsync(data);
//            //await writer.FlushAsync();
//        //});
//#endif
//    }

//    public byte[] RecieveData()
//    {
//#if UNITY_UWP
//        byte[] output = new byte[len];
//        Task.Run(async () =>
//        {
//            /*await */st.Read(output, 0, len);
//        });
//        return output;
//#else
//        byte[] output = new byte[0];
//        return output;
//#endif


//    }
//}

namespace HoloToolkit.Unity {
    public class GameBehavior : MonoBehaviour, IInputClickHandler
    {
        //TcpNetworkClientManager tncm = new TcpNetworkClientManager("192.168.0.4", 4000);

        [DllImport("CppPlugin")]
        unsafe private static extern void Hengento(
        //unsafe private static extern IntPtr Hengento(
            IntPtr src_ptr, int src_rows, int src_cols,
            int rows_start, int rows_end, int cols_start, int cols_end,
            byte[] output, int dst_rows, int dst_cols,
            double[] debug_log, bool TimeRecord,
            float cvt_R = 0.299f, float cvt_G = 0.587f, float cvt_B = 0.114f);

        [DllImport("CppPlugin")]
        unsafe private static extern void Crop(
            byte* data, int src_rows, int src_cols,
            int rows_start, int rows_end, int cols_start, int cols_end,
            byte[] output, int dst_rows, int dst_cols,
            bool TimeRecord);

        private PhotoCapture capture = null;
        private Resolution resolution;
        private CameraParameters c;
        private Texture2D tex;

        private GameObject textureView;
        private GameObject ViewObj;
        private RawImage rawImage;
        private GameObject LogMenu;
        private LogMenuBehavior LogMenuBeh;
        private GameObject textureViewInst;



        //int dst_rows = 600; // 200;
        //int dst_cols = 600; // 200;

        int dst_rows = 1152;
        int dst_cols = 2048;

        int src_rows, src_cols;
        int rows_start, rows_end, cols_start, cols_end;

        bool startFlg = false;
        //bool TapCapture = false;
        bool TapCapture = false;
        double[] debug_log = new double[100];

        int MODE = 0; // 変幻灯モード
                      //int MODE = 1; // 画風変換モード]


        private GameObject SpatialMapping;
        public Material ReprojectionMaterial;

        //private MediaCapture _mediaCapture;

        //private async Task InitCameraAsync()
        //{
        //    _mediaCapture = new MediaCapture();
        //    try
        //    {
        //        await _mediaCapture.InitializeAsync();
        //    }catch (UnauthorizedAccessException)
        //    {
        //        Debug.Log("The app was denied access to the camera");
        //    }
        //}

        void SpatialMappingInitSettings()
        {
            SpatialMapping = GameObject.Find("SpatialMapping").gameObject;
            var spatialMappingSources = SpatialMapping.GetComponents<SpatialMappingSource>();
            foreach (var source in spatialMappingSources)
            {
                //source.SurfaceAdded += SpatialMappingSource_SurfaceAdded;
                //source.SurfaceUpdated += SpatialMappingSource_SurfaceUpdated;
            }
        }

        void Start()
        {
            if (TapCapture) InputManager.Instance.PushFallbackInputHandler(gameObject);
            //SpatialMappingInitSettings();

            LogMenu = GameObject.Find("LogMenu").gameObject;
            LogMenu.GetComponentInChildren<Text>().color = Color.green;
            LogMenuBeh = LogMenu.GetComponent<LogMenuBehavior>();
            LogMenu.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            LogMenu.transform.position = new Vector3(0, 0, 3);
            LogMenu.transform.localScale = new Vector3(0.01f, 0.01f, 3);

            //textureView = GameObject.Find("TextureView").gameObject;
            //textureView.GetComponent<RectTransform>().sizeDelta = new Vector2(dst_cols, dst_rows);
            //ViewObj = textureView.transform.Find("RawImage").gameObject;
            //textureView.transform.localScale = new Vector3(0.0015f, 0.0015f, 1);
            //ViewObj.transform.localScale = new Vector3(-1, -1, 1);
            //rawImage = ViewObj.GetComponent<RawImage>();

            textureViewInst = (GameObject)Resources.Load("TextureView");
            textureViewInst.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(dst_cols, dst_rows);



            for (int i=0; i < 100; i++)
            {
                debug_log[i] = -1;
            }

            Debug.Log("Start Settings");

            if (!TapCapture)
                PhotoCapture.CreateAsync(false, (_capture) =>
                {
                    capture = _capture;
                    resolution = PhotoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).First();

                    src_cols = resolution.width;
                    src_rows = resolution.height;
                    Debug.Log(src_cols + " " + src_rows);

                    rows_start = (src_rows - dst_rows) / 2;
                    rows_end = rows_start + dst_rows;
                    cols_start = (src_cols - dst_cols) / 2;
                    cols_end = cols_start + dst_cols;

                    tex = new Texture2D(src_cols, src_rows, TextureFormat.BGRA32, false);
                    c = new CameraParameters();
                    c = new CameraParameters(WebCamMode.PhotoMode);

                    c.hologramOpacity = 1.0f;
                    c.cameraResolutionWidth = src_cols;
                    c.cameraResolutionHeight = src_rows;
                    c.pixelFormat = CapturePixelFormat.BGRA32;

                    capture.StartPhotoModeAsync(c, OnPhotoModeStarted);

                });

        }


        int frame = 0;
        int FRAME_RATE = 1;

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(startFlg);
            //++frame;

            // Rayを描く
            Debug.DrawRay(GazeManager.Instance.GazeOrigin, GazeManager.Instance.GazeNormal, Color.red);

            // SpatialMappingの作成が終わったら，も含めたい．
            if (startFlg)
                capture.TakePhotoAsync(OnCapturedPhotoToMemory);
            //if (frame > FRAME_RATE)
            //{
            //    frame = 0;
            //    if (!TapCapture)
            //    {

            //    }
            //}
        }



        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            //Debug.Log(result.success + "Second Started");
            if (result.success)
            {

                if (TapCapture)
                {
                    string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
                    string filePath = System.IO.Path.Combine(PictureFileDirectoryPath(), filename);

                    capture.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
                }
                else startFlg = true;
                Debug.Log("PhotoMode started");

            }

            else
            {
                Debug.LogError("Unable to start photo mode!");
                Debug.Log("Unable to start photo mode!");
            }
        }


        void SpatialMapping_SurfaceAdded()
        {

        }


        /// <summary>
        /// 画像保存ディレクトリパス
        /// 実行環境によって参照ディレクトリを変更する
        /// </summary>
        private string PictureFileDirectoryPath()
        {
            string directorypath = "";
#if UNITY_UWP
            // HoloLens上での動作の場合、LocalAppData/AppName/LocalStateフォルダを参照する
            directorypath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
        // Unity上での動作の場合、Assets/StreamingAssetsフォルダを参照する
        directorypath = UnityEngine.Application.streamingAssetsPath;
#endif
            return directorypath;
        }


        void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                Debug.Log("Saved Photo to disk!");
                capture.StopPhotoModeAsync(OnStoppedPhotoMode);
            }
            else
            {
                Debug.Log("Failed to save Photo to disk");
            }
        }


        bool debugFlg = true;

        bool TimeRecord = false;
        float cvt_R = 0.299f;
        float cvt_G = 0.587f;
        float cvt_B = 0.114f;
        RaycastHit Hit;
        int num = 0;
        GameObject Obj;


        private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            Texture2D texOutput = new Texture2D(dst_cols, dst_rows, TextureFormat.BGRA32, false);

            //#if WINDOWS_UWP

            // RayCast情報を取得する

            bool res = Physics.Raycast(GazeManager.Instance.GazeOrigin, GazeManager.Instance.GazeNormal, out Hit, 20f, SpatialMappingManager.Instance.LayerMask);
            //Debug.Log(result.success + " " + res);

            if (result.success && res)
            {
                string str = "";


                GameObject spatialMapping = GameObject.Find(Hit.collider.name).gameObject;

                // Stopwatchクラス生成
                var sw = new System.Diagnostics.Stopwatch();

                //Debug.Log(Hit.point);return;
                //Debug.Log(Hit.point + " " + GameObject.Find("DefaultCursor").gameObject.transform.position);
                //str += Hit.point + " " + GameObject.Find("DefaultCursor").gameObject.transform.position;


                // Rayの描画はUpdate()内でした．

                // RayCastのあった位置にTextureViewInstをInstantiateする．
                str += "衝突物の法線: " + Hit.normal + "\n";
                str += "カメラの目線" + GazeManager.Instance.GazeNormal + "\n";



#if UNITY_EDITOR
                photoCaptureFrame.UploadImageDataToTexture(texOutput);

                //RawImage ri = Obj.transform.Find("RawImage").gameObject.AddComponent<RawImage>();
                //Debug.Log(ri);
                //ri.texture = texOutput;
                //Obj = null; ri = null;

                //Matrix4x4 cameraToWorldMatrix, worldToCameraMatrix, projectionMatrix;
                //photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
                //worldToCameraMatrix = cameraToWorldMatrix.inverse;
                //photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

                //renderer.sharedMaterial = 

                //SpatialMappingManager.Instance.


                //spatialMapping.



                //SpatialMappingManager.Instance.SetSurfaceMaterial(ReprojectionMaterial);

                //Vector3[] vertices = mesh.vertices;
                //Vector2[] uvs = new Vector2[vertices.Length];
                //Vector2 ScreenPosition;
                //for (int i = 0; i < uvs.Length; i++)
                //{
                //    ScreenPosition = cam.WorldToViewportPoint(Obj.transform.TransformPoint(vertices[i]));
                //    uvs[i].Set(ScreenPosition.x, ScreenPosition.y);
                //}
                //mesh.uv = uvs;









#endif

                // 計測開始
#if UNITY_UWP

                unsafe
                {

                    switch (MODE) {
                        /* 変幻灯モード */
                        case 0:

                            if (Obj != null) { Destroy(Obj); Obj = null; }
                            // テクスチャを貼るオブジェクトを生成
                            Obj = Instantiate(textureViewInst, Hit.point, Quaternion.identity);
                            Obj.transform.position = Hit.point;
                            Obj.transform.rotation = GameObject.Find("DefaultCursor").gameObject.transform.rotation;


                            // Rayが当たる位置を中心に切り取っていくのがよいけど，どうやって中心の当たり判定をするんだろう？
                            IntPtr src_ptr = photoCaptureFrame.GetUnsafePointerToBuffer();
                            int size = Marshal.SizeOf<byte>() * 4 * dst_rows * dst_cols;
                            byte[] output = new byte[4 * dst_rows * dst_cols];

                            sw.Restart();
                            Hengento(
                                src_ptr, src_rows, src_cols,
                                rows_start, rows_end, cols_start, cols_end,
                                output, dst_rows, dst_cols,
                                debug_log, true,
                                cvt_R, cvt_G, cvt_B);
                            Marshal.Release(src_ptr);

                            if (TimeRecord)
                            {
                                int i = 1;
                                for (i = 1; i < 100; i++)
                                {
                                    if (debug_log[i] == -1) break;
                                    str += debug_log[i] + "ミリ秒\n";
                                    debug_log[i] = -1;
                                }
                                str += "data: " + debug_log[i + 1] + " ptr: " + debug_log[i + 2] + "\n";
                            }

                            if (TimeRecord)
                            {
                                sw.Stop();
                                str += "Hengento " + sw.ElapsedMilliseconds + "ミリ秒\n";
                                sw.Restart();
                            }

                            texOutput.LoadRawTextureData(output);

                            if (TimeRecord)
                            {
                                sw.Stop();
                                str += "LoadRawTextureData " + sw.ElapsedMilliseconds + "ミリ秒\n";
                                sw.Restart();
                            }

                            texOutput.Apply();
                            texOutput.wrapMode = TextureWrapMode.Clamp;

                            if (TimeRecord)
                            {
                                sw.Stop();
                                str += "texOutput.Apply  " + sw.ElapsedMilliseconds + "ミリ秒\n";
                                sw.Restart();
                            }


                            // オブジェクトの位置
                            GameObject ri = Obj.transform.Find("RawImage").gameObject;
                            ri.GetComponent<RectTransform>().sizeDelta = new Vector2(dst_cols, dst_rows);
                            ri.AddComponent<RawImage>().texture = texOutput;
                            //Debug.Log(ri);
                            Obj = null; ri = null;

                            // とりあえず普通に配置してみて．

                            //RefMaterial(texOutput);

                            //if (TimeRecord)
                            //{
                            sw.Stop();
                            //str += "RefMaterial " + sw.ElapsedMilliseconds + "ミリ秒\n";
                            str += "All " + sw.ElapsedMilliseconds + "ミリ秒\n";
                            //}

                            break;

                        /* 画風変換モード */
                        case 1:
                            //Crop(ptr, src_rows, src_cols,
                            //    rows_start, rows_end, cols_start, cols_end,
                            //    output, dst_rows, dst_cols,
                            //    TimeRecord);


                            // TCPを使って送る
                            //tncm.SendData(output);
                            //output = tncm.RecieveData();

                            //texOutput.LoadRawTextureData(output);
                            //texOutput.Apply();
                            //RefMaterial(texOutput);


                            /* Spatial Mappingのメッシュにテクスチャをつける */
                            var mesh = spatialMapping.GetComponent<MeshFilter>().mesh;

                            //これは本来 Start()とか最初の関数で呼び出す．何度も呼び出すことに意味はない
                            mesh.MarkDynamic();

                            int[] triangles = mesh.triangles;

                            Vector3[] vertices = mesh.vertices;
                            //List<Vector2> uvs = new List<Vector2>();
                            Vector2[] uvs = new Vector2[vertices.Length];
                            Vector2 ScreenPosition;

                            Camera cam = Camera.main;

                            for (int i = 0; i < vertices.Length; i++)
                            {
                                ScreenPosition = cam.WorldToViewportPoint(spatialMapping.transform.TransformPoint(vertices[i]));
                                uvs[i].Set(ScreenPosition.x, ScreenPosition.y);
                                /*if (ScreenPosition.x >= 0 && ScreenPosition.x <= 1 && ScreenPosition.y >= 0 && ScreenPosition.y <= 1) */
                            }

                            //Debug.Log(vertices.Length + " " + uvs.Length);
                            mesh.uv = uvs;

                            // マテリアルを変更しても，UWPだと反映されないー＞なぜ？調査．
                            //Material material = new Material(Shader.Find("Diffuse"));
                            Material material = (Material)Resources.Load("PictTestMaterial");
                            material.mainTexture = texOutput;
                            spatialMapping.GetComponent<MeshRenderer>().material = material;


                            break;


                    }
                }

#endif



                // SpatialMappingの全ての子オブジェクトの内，Rayが当たるものに対して
                // メッシュの全点に対して，transform.TransformPoint(v)でワールド座標系に直す
                // WorldToViewportPoint でカメラから見たスクリーン内の位置に直す．
                // スクリーン内に入っていれば，uv座標系の対応点を決定する．


                LogMenuBeh.UpdateAppText(str);
            }

        }




        void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            capture.Dispose();
            capture = null;
        }

        /// <summary>
        /// テクスチャをマテリアルに反映する
        /// </summary>
        public void RefMaterial(Texture2D texture/*, GameObject obj*/)
        {
            //RawImage rawImg = obj.GetComponentInChildren<RawImage>();
            //rawImg.texture = texture;
            rawImage.texture = texture;
        }



        /// <summary>
        /// クリックイベント
        /// </summary>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            // キャプチャを開始する
            PhotoCapture.CreateAsync(false, (_capture) =>
            {
                capture = _capture;
                resolution = PhotoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).First();
                src_cols = resolution.width;
                src_rows = resolution.height;

                tex = new Texture2D(src_cols, src_rows);
                c = new CameraParameters(WebCamMode.PhotoMode);
                c.hologramOpacity = 1.0f;
                c.cameraResolutionWidth = src_cols;
                c.cameraResolutionHeight = src_rows;
                c.pixelFormat = CapturePixelFormat.BGRA32;
                capture.StartPhotoModeAsync(c, OnPhotoModeStarted);
            });
        }





    }

}
 