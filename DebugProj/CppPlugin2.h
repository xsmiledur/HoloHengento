#pragma once

/* OpenCV */
#include "opencv2\opencv.hpp"
#define CV_VERSION_STR CVAUX_STR(CV_MAJOR_VERSION) CVAUX_STR(CV_MINOR_VERSION) CVAUX_STR(CV_SUBMINOR_VERSION)
#ifdef _DEBUG
#define CV_EXT_STR "d.lib"
#else
#define CV_EXT_STR ".lib"
#endif
#pragma comment(lib, "opencv_world"			CV_VERSION_STR CV_EXT_STR)

#include <Windows.h>
#include <omp.h>

using namespace std;

//#include <omp.h>

class CppPlugin2
{
private:
	LARGE_INTEGER freq, start, end;

public:
	CppPlugin2();
	~CppPlugin2();
	
	void Hengento(
		unsigned char* data, int src_rows, int src_cols,
		int rows_start, int rows_end, int cols_start, int cols_end,
		unsigned char* output, int dst_rows, int dst_cols,
		int* debug_log, bool TimeRecord, int no,
		const float cvt_R, const float cvt_G, const float cvt_B
		);

	void Crop(
		uchar* data, int src_rows, int src_cols,
		int rows_start, int rows_end, int cols_start, int cols_end,
		uchar* output, int dst_rows, int dst_cols,
		bool TimeRecord, int no
	);

};

