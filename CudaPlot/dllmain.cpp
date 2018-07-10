#include "stdafx.h"

#define DllExport  extern "C" __declspec( dllexport ) 

bool ready = false;

int range[4];

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


uchar4 UInt32_to_uchar4(uint32_t val)
{
	uchar4 result;

	result.x = (uint8_t)(val & 0x000000ff);
	result.y = (uint8_t)((val & 0x0000ff00) >> 8);
	result.z = (uint8_t)((val & 0x00ff0000) >> 16);
	result.w = (uint8_t)((val & 0xff000000) >> 24);

	return result;
}

DllExport bool InitCudaPlot(int chartRows, int chartCols, int chartArrayWidth, int chartArrayHeight, 
	int margin, int padding, int aggregateWidth, int aggregateHeight,
	uint32_t windowBkgColor,
	uint32_t chartBkgColor, uint32_t chartSelectedColor, uint32_t chartFrameColor, uint32_t chartAxisColor, uint32_t chartPlotColor,
	int xmin, int xmax, int ymin, int ymax,	int maxNumDataPoints, int numTraces, CudaChartArray** pp_chartArray)
{
	bool ready = true;

	int2 xRange = { xmin,xmax };
	int2 yRange = { ymin,ymax };

	uchar4 col1 = UInt32_to_uchar4(windowBkgColor);
	uchar4 col2 = UInt32_to_uchar4(chartBkgColor);
	uchar4 col3 = UInt32_to_uchar4(chartSelectedColor);
	uchar4 col4 = UInt32_to_uchar4(chartFrameColor);
	uchar4 col5 = UInt32_to_uchar4(chartAxisColor);
	uchar4 col6 = UInt32_to_uchar4(chartPlotColor);

	*pp_chartArray = new CudaChartArray(chartRows, chartCols, chartArrayWidth, chartArrayHeight, margin, padding,
									aggregateWidth, aggregateHeight,
									col1,col2,col3,col4,col5,col6, xRange, yRange, maxNumDataPoints, numTraces);

	return ready;
}



DllExport void Shutdown(CudaChartArray* p_chart_array)
{
	delete p_chart_array;
}




DllExport int2 GetChartArrayPixelSize(CudaChartArray* pChartArray)
{	
	int2 size = pChartArray->GetChartArrayPixelSize();
	return size;
}


DllExport void Resize(CudaChartArray* pChartArray, int chartArrayWidth, int chartArrayHeight, int aggregateWidth, int aggregateHeight)
{
	pChartArray->Resize(chartArrayWidth, chartArrayHeight, aggregateWidth, aggregateHeight);
}


DllExport void SetSelected(CudaChartArray* pChartArray)
{
	int rows = pChartArray->m_rows;
	int cols = pChartArray->m_cols;
	int num = rows*cols;

	pChartArray->SetSelected();
}


DllExport void SetPlotColor(CudaChartArray* pChartArray, uint32_t color)
{
	uchar4 col1 = UInt32_to_uchar4(color);
	pChartArray->SetPlotColor(col1);
}


DllExport void AppendData(CudaChartArray* pChartArray, int* xArray, int* yArray, int numPoints, int traceNum)
{	
	int2* newPoints = (int2*)malloc(numPoints * sizeof(int2));

	for (int i = 0; i < numPoints; i++)
	{
		newPoints[i].x = xArray[i];
		newPoints[i].y = yArray[i];
	}

	pChartArray->AppendData(newPoints, traceNum);

	free(newPoints);
}

DllExport void Redraw(CudaChartArray* pChartArray)
{
	pChartArray->Redraw();
}

DllExport void RedrawAggregate(CudaChartArray* pChartArray)
{
	pChartArray->RedrawAggregate();
}

DllExport void* GetChartImagePtr(CudaChartArray* pChartArray)
{
	return (void*)pChartArray->GetChartImagePtr();
}

DllExport void* GetRangePtr(CudaChartArray* pChartArray)
{
	range[0] = pChartArray->m_x_min;
	range[1] = pChartArray->m_x_max;
	range[2] = pChartArray->m_y_min;
	range[3] = pChartArray->m_y_max;

	return (void*)&range;
}


DllExport void* GetAggregateImagePtr(CudaChartArray* pChartArray)
{
	return (void*)pChartArray->GetAggregateImagePtr();
}


DllExport void* GetSelectionArrayPtr(CudaChartArray* pChartArray)
{
	return (void*)pChartArray->GetSelectionArrayPtr();
}


DllExport void SetWindowBackground(CudaChartArray* pChartArray, uchar4 color)
{
	pChartArray->SetWindowBackground(color);
}


DllExport void SetRanges(CudaChartArray* pChartArray, int xmin, int xmax, int ymin, int ymax)
{
	pChartArray->SetRanges(xmin, xmax, ymin, ymax);
}


DllExport int32_t GetRowFromY(CudaChartArray* pChartArray, int32_t y)
{
	return pChartArray->GetRowFromY(y);
}

DllExport int32_t GetColumnFromX(CudaChartArray* pChartArray, int32_t x)
{
	return pChartArray->GetColumnFromX(x);
}

DllExport void Reset(CudaChartArray* pChartArray)
{
	pChartArray->Reset();
}
