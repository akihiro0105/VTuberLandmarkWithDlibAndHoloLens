// SenderLandmark.cpp : このファイルには 'main' 関数が含まれています。プログラム実行の開始と終了がそこで行われます。
//

#include "pch.h"
#include <iostream>

// 物体検出と特徴点認識で取得したデータをUDPで送信
// キーボードで閾値を設定可能
// e : 目の最大サイズを指定
// m : 口の最大サイズを指定

#pragma comment(lib,"ws2_32.lib")
#include<WinSock2.h>

#include <dlib/opencv.h>
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <dlib/image_processing/frontal_face_detector.h>
#include <dlib/image_processing/render_face_detections.h>
#include <dlib/image_processing.h>
#include <dlib/gui_widgets.h>

#include<Windows.h>

using namespace dlib;
using namespace std;

int main(int argc, char* argv[])
{
	try
	{
		char* buf;
		int c = strtol(argv[1], &buf, 10);
		cv::VideoCapture cap = (*buf != '\0') ? cv::VideoCapture(argv[1]) : cv::VideoCapture(c);
		if (!cap.isOpened())
		{
			cerr << "Unable to connect to camera" << endl;
			return 1;
		}

		image_window win;

		shape_predictor pose_model;
		deserialize(argv[2]) >> pose_model;

		typedef scan_fhog_pyramid<pyramid_down<6> > image_scanner_type;
		object_detector<image_scanner_type> detector2;
		deserialize(argv[3]) >> detector2;


		WSAData wsaData;

		SOCKET sock;
		struct sockaddr_in addr;

		WSAStartup(MAKEWORD(2, 0), &wsaData);

		sock = socket(AF_INET, SOCK_DGRAM, 0);

		addr.sin_family = AF_INET;
		addr.sin_port = htons(8080);
		addr.sin_addr.S_un.S_addr = inet_addr(argv[4]);


		float mouse = 0.0f,eye=0.0f;// 0,1 2,3 5,6
		float mouseC = 0.0f, eyeC = 0.0f;
		float mousebuf = 0.0f, eyebuf = 0.0f;

		// Grab and process frames until the main window is closed by the user.
		while (!win.is_closed())
		{
			cv::Mat temp;
			if (!cap.read(temp))
			{
				break;
			}
			cv_image<bgr_pixel> cimg(temp);

			// haneru face
			std::vector<rectangle> dets = detector2(cimg);
			std::vector<full_object_detection> shapes;
			for (unsigned long i = 0; i < dets.size(); ++i) {
				shapes.push_back(pose_model(cimg, dets[i]));
			}

			// Display it all on the screen
			win.clear_overlay();
			win.set_image(cimg);

			// 顔部位認識箇所を丸で描画
			std::vector<image_window::overlay_circle> circle;
			if (shapes.size() > 0)
			{
				for (unsigned long i = 0; i < shapes.size(); i++)
				{
					const full_object_detection& d = shapes[i];
					for (unsigned long j = 0; j < d.num_parts(); j++)
					{
						circle.push_back(image_window::overlay_circle(d.part(j), 2.5, rgb_pixel(0, 255, 0)));
					}

					float x = d.part(6).x() - d.part(7).x();
					float y = d.part(6).y() - d.part(7).y();
					mouse = x * x + y * y;

					mouseC = mouse / mousebuf;
					if (mouseC > 1.0f)mouseC = 1.0f;

					x = d.part(0).x() - d.part(1).x();
					y= d.part(0).y() - d.part(1).y();
					float leftC = x * x + y * y;
					x = d.part(2).x() - d.part(3).x();
					y = d.part(2).y() - d.part(3).y();
					float rightC = x * x + y * y;

					eye = (leftC + rightC) / 2;
					eyeC = eye / eyebuf;
					if (eyeC > 1.0f)eyeC = 1.0f;
					eyeC = 1.0f - eyeC;
				}
				win.add_overlay(circle);

				char* data = new char[256];
				sprintf(data, "{\"eye\":%f,\"mouse\":%f}", eyeC, mouseC);
				int len = strlen(data);
				printf("%s\n", data);
				sendto(sock, data, len, 0, (struct sockaddr *)&addr, sizeof(addr));
			}
			if (GetAsyncKeyState('E'))
			{
				eyebuf = eye;
			}
			if (GetAsyncKeyState('M'))
			{
				mousebuf = mouse;
			}
		}

		closesocket(sock);
		WSACleanup();
	}
	catch (serialization_error& e)
	{
		cout << "You need dlib's default face landmarking model file to run this example." << endl;
		cout << "You can get it from the following URL: " << endl;
		cout << "   http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2" << endl;
		cout << endl << e.what() << endl;
	}
	catch (exception& e)
	{
		cout << e.what() << endl;
	}
}

// プログラムの実行: Ctrl + F5 または [デバッグ] > [デバッグなしで開始] メニュー
// プログラムのデバッグ: F5 または [デバッグ] > [デバッグの開始] メニュー

// 作業を開始するためのヒント: 
//    1. ソリューション エクスプローラー ウィンドウを使用してファイルを追加/管理します 
//   2. チーム エクスプローラー ウィンドウを使用してソース管理に接続します
//   3. 出力ウィンドウを使用して、ビルド出力とその他のメッセージを表示します
//   4. エラー一覧ウィンドウを使用してエラーを表示します
//   5. [プロジェクト] > [新しい項目の追加] と移動して新しいコード ファイルを作成するか、[プロジェクト] > [既存の項目の追加] と移動して既存のコード ファイルをプロジェクトに追加します
//   6. 後ほどこのプロジェクトを再び開く場合、[ファイル] > [開く] > [プロジェクト] と移動して .sln ファイルを選択します
