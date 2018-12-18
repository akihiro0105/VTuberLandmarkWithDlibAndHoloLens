# VTuberLandmarkWithDlibAndHoloLens
PC上のdlibでVTuberの表情認識を行い，表情データをHoloLensに送信して表示する

# Environment
- Windows10
    + Visual Studio 2017
- Hololens
    + Unity 2017.4.16f1

# Install library
- Windows10
    + cmake-3.13.2-win64-x64 [https://cmake.org/](https://cmake.org/)
    + dlib-19.16 [http://dlib.net/](http://dlib.net/)
    + opencv-3.4.4-vc14_vc15 [https://opencv.org/](https://opencv.org/)

- HoloLens
    + HoloLensModule [https://github.com/akihiro0105/HoloLensModule](https://github.com/akihiro0105/HoloLensModule)

# How to use
[ブログのurl](https://link)

# Project
- ReceiveLandmark
    + UDPで受信したlandmarkデータを表示する

- SenderLandmark
 + 動画またはWebカメラからの映像を取得してdlibによるlandmark認識を行う
 + landmark認識を指定IPアドレスにUDPによるlandmarkデータの送信を行う
 + キーボードのeキーで瞼の最大サイズを設定，mキーで口の最大サイズを設定