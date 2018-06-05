// DebugProj.cpp : アプリケーションのエントリ ポイントを定義します。
//

#pragma hdrstop
#include <tchar.h>
#include <windows.h>
#include <iostream>
#include <conio.h>

#include "CppPlugin2.h"

using namespace std;


int main()
{
	LARGE_INTEGER freq, start, end;

	int src_rows, src_cols;
	int dst_rows = 400;
	int dst_cols = 400;

	const float cvt_R = 0.299;
	const float cvt_G = 0.587;
	const float cvt_B = 0.114;

	bool TimeRecord = false;

	CppPlugin2 cp;

	// BGRA32で読み込む？
	vector<string> pics = vector<string>{
		"CapturedImage14.07022_n.jpg",
		"CapturedImage16.81768_n.jpg",
		"CapturedImage18.38228_n.jpg",
		"CapturedImage20.3811_n.jpg",
		"CapturedImage23.19668_n.jpg",
		"CapturedImage25.21103_n.jpg",
	};


	if (!QueryPerformanceFrequency(&freq)) return 0;


	for (int i = 0; i < pics.size(); i++) {
		cv::Mat mat = cv::imread("Pictures/" + pics[i]);
		cv::Mat mat_;
		cv::cvtColor(mat, mat_, CV_BGR2BGRA);
		src_rows = mat.rows;
		src_cols = mat.cols;

		uchar output[100];
		int debug_log[100];

		int rows_start = (src_rows - dst_rows) / 2;
		int rows_end = rows_start + dst_rows;
		int cols_start = (src_cols - dst_cols) / 2;
		int cols_end = cols_start + dst_cols;
		std::cout << src_rows << " " << src_cols << endl;
		std::cout << rows_start << " " << rows_end << " " << cols_start << " " << cols_end << " " << endl;

		if (!QueryPerformanceCounter(&start)) return 0;
		cp.Hengento(mat_.data, src_rows, src_cols,
			rows_start, rows_end, cols_start, cols_end,
			output, dst_rows, dst_cols,
			debug_log, false, i,
			cvt_R, cvt_G, cvt_B
			);

		if (!QueryPerformanceCounter(&end)) return 0;
		cout << "duration = " << (double)1000 * (end.QuadPart - start.QuadPart) / freq.QuadPart << "msec" << endl;
		cout << endl;

		if (!QueryPerformanceCounter(&start)) return 0;
		cp.Crop(mat.data, src_rows, src_cols,
			rows_start, rows_end, cols_start, cols_end,
			output, dst_rows, dst_cols,
			false, i);

		if (!QueryPerformanceCounter(&end)) return 0;
		cout << "duration = " << (double)1000 * (end.QuadPart - start.QuadPart) / freq.QuadPart << "msec" << endl;
		cout << endl;
	}
	
	return 0;

}

