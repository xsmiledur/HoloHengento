#include "CppPlugin2.h"



CppPlugin2::CppPlugin2()
{
}


CppPlugin2::~CppPlugin2()
{
}

void CppPlugin2::Hengento(
	unsigned char* data, int src_rows, int src_cols,
	int rows_start, int rows_end, int cols_start, int cols_end,
	unsigned char* output, int dst_rows, int dst_cols,
	int* debug_log, bool TimeRecord, int no,
	const float cvt_R, const float cvt_G, const float cvt_B)

{
#ifdef _OPENMP
	//cout << "OpenMP : Enabled (Max # of threads = " << omp_get_max_threads() << ")" << endl;
	omp_set_num_threads(omp_get_max_threads());
#endif
	if (!QueryPerformanceFrequency(&freq)) return;



	cv::Mat imgBin(dst_rows, dst_cols, CV_8U, cv::Scalar::all(255));
	cv::Mat1b imgGus(dst_rows, dst_cols);
	cv::Mat4b imgLap(dst_rows, dst_cols, cv::Scalar::all(128));

	unsigned char* src_ptr = data + 4 * src_cols * rows_start + 4 * cols_start;

	if (TimeRecord) if (!QueryPerformanceCounter(&start)) return;
#pragma omp parallel for
	for (int j = 0; j < dst_rows; j++) {
		uchar* src_ptr_ = src_ptr + 4 * src_cols * j;
		uchar* bin_ptr_ = imgBin.ptr<uchar>(j);
		for (int i = 0; i < dst_cols; i++) {
			*(bin_ptr_) = (uchar)(int)((float)*src_ptr_ * cvt_B + (float)*(src_ptr_ + 1) * cvt_G + (float)*(src_ptr_ + 2) * cvt_R);
			//if ((float)*src_ptr_ * cvt_B + (float)*(src_ptr_ + 1) * cvt_G + (float)*(src_ptr_ + 2) * cvt_R < 128) *(bin_ptr_) = (uchar)0;
			++bin_ptr_; src_ptr_ += 4;
		}
	}
	if (TimeRecord) {
		if (!QueryPerformanceCounter(&end)) return;
		cout << "duration = " << (double)1000 * (end.QuadPart - start.QuadPart) / freq.QuadPart << "msec" << endl;
	}
	cv::imshow("bin", imgBin);
	cv::waitKey(10);
	int cols = dst_cols;
	// ガウシアンフィルタによる平滑化
	if (TimeRecord) if (!QueryPerformanceCounter(&start)) return;
#pragma omp parallel for
	for (int j = 0; j < dst_rows; j++) {
		uchar* bin = imgBin.ptr<uchar>(j);
		uchar* gus_ptr_ = imgGus.ptr<uchar>(j);
		for (int i = 0; i < dst_cols; i++) {
			if (i == 0 || j == 0 || i == dst_cols - 1 || j == dst_rows - 1) *gus_ptr_ = 0;
			else {
				int ptr = (int)*(bin - 2 * cols - 2);
				ptr += (int)*(bin - 2 * cols - 1) * 4;
				ptr += (int)*(bin - 2 * cols) * 6;
				ptr += (int)*(bin - 2 * cols + 1) * 4;
				ptr += (int)*(bin - 2 * cols + 2);
				
				ptr += (int)*(bin - cols - 2) * 4;
				ptr += (int)*(bin - cols - 1) * 16;
				ptr += (int)*(bin - cols) * 24;
				ptr += (int)*(bin - cols + 1) * 16;
				ptr += (int)*(bin - cols + 2) * 4;
				
				ptr += (int)*(bin - 2) * 6;
				ptr += (int)*(bin - 1) * 24;
				ptr += (int)*(bin) * 36;
				ptr += (int)*(bin + 1) * 24;
				ptr += (int)*(bin + 2) * 6;

				ptr += (int)*(bin + cols - 2) * 4;
				ptr += (int)*(bin + cols - 1) * 16;
				ptr += (int)*(bin + cols) * 24;
				ptr += (int)*(bin + cols + 1) * 16;
				ptr += (int)*(bin + cols + 2) * 4;

				ptr += (int)*(bin + 2 * cols - 2);
				ptr += (int)*(bin + 2 * cols - 1) * 4;
				ptr += (int)*(bin + 2 * cols) * 6;
				ptr += (int)*(bin + 2 * cols + 1) * 4;
				ptr += (int)*(bin + 2 * cols + 2);

				*gus_ptr_ = (uchar)(ptr / 256);
			}
			++bin; ++gus_ptr_;
		}
	}
	if (TimeRecord) {
		if (!QueryPerformanceCounter(&end)) return;
		cout << "duration = " << (double)1000 * (end.QuadPart - start.QuadPart) / freq.QuadPart << "msec" << endl;
		cout << endl;
	}

	cv::imshow("gus", imgGus);
	cv::waitKey(10);


	// ラプラシアンフィルタによるエッジ検出
	if (TimeRecord) if (!QueryPerformanceCounter(&start)) return;
#pragma omp parallel for
	for (int j = 0; j < dst_rows; j++) {
		uchar* gus_ptr_ = imgGus.ptr<uchar>(j);
		uchar* lap_ptr_ = imgLap.ptr<uchar>(j);
		for (int i = 0; i < dst_cols; i++) {
			if (i == 0 || j == 0 || i == dst_cols - 1 || j == dst_rows - 1 ||
				(int)*(gus_ptr_ - dst_cols) + (int)*(gus_ptr_ - 1) + (int)*(gus_ptr_ + 1) + (int)*(gus_ptr_ + dst_cols) - 4 * (int)*(gus_ptr_) <= 3)
				*(lap_ptr_ + 3) = 0;
			else *(lap_ptr_ + 3) = 255;
			++gus_ptr_; lap_ptr_ += 4;
		}
	}
	if (TimeRecord) {
		if (!QueryPerformanceCounter(&end)) return;
		cout << "duration = " << (double)1000 * (end.QuadPart - start.QuadPart) / freq.QuadPart << "msec" << endl;
		cout << endl;
	}

	cv::imshow("lap", imgLap);
	cv::imwrite(to_string(no) + "_lap.png", imgLap);
	cv::waitKey(1000);

	output = data;
	
	return;
}


void CppPlugin2::Crop(
	uchar* data, int src_rows, int src_cols,
	int rows_start, int rows_end, int cols_start, int cols_end,
	uchar* output, int dst_rows, int dst_cols,
	bool TimeRecord, int no
)
{
#ifdef _OPENMP
	//cout << "OpenMP : Enabled (Max # of threads = " << omp_get_max_threads() << ")" << endl;
	omp_set_num_threads(omp_get_max_threads());
#endif
	if (TimeRecord) if (!QueryPerformanceCounter(&freq)) return;

	unsigned char* src_ptr = data + 3 * src_cols * rows_start + 3 * cols_start;
	cv::Mat3b imgCrp(dst_rows, dst_cols);

	if (TimeRecord) if (!QueryPerformanceCounter(&start)) return;
#pragma omp parallel for
	for (int j = 0; j < dst_rows; j++) {
		uchar* src_ptr_ = src_ptr + 3 * src_cols * j;
		uchar* crp_ptr_ = imgCrp.ptr<uchar>(j);
		for (int i = 0; i < 3 * dst_cols; i++) {
			*crp_ptr_ = *src_ptr_; ++crp_ptr_; ++src_ptr_;
		}
	}
	if (TimeRecord) {
		if (!QueryPerformanceCounter(&end)) return;
		cout << "duration... = " << (double)1000 * (end.QuadPart - start.QuadPart) / freq.QuadPart << "msec" << endl;
	}
	cv::imwrite("crop_" + to_string(no) + ".png", imgCrp);
	output = imgCrp.data;

}

//int __stdcall Add(int a, int b)
//{
//	return a + b;
//}
//int __stdcall Sum(unsigned char* data, int height, int width)
//{
//	cv::Mat mat(height, width, CV_8U, data);
//	cv::Scalar sum_4ch = cv::sum(mat);
//	return static_cast<int>(sum_4ch[0]);
//}
