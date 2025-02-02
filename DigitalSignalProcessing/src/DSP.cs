﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DigitalSignalProcessing
{
    public static class DSP
    {
        /// <summary>
        /// Calculates the mean and sample standard deviation of a signal.
        /// </summary>
        public static (double mean, double std) CalcMeanStd(double[] x)
        {
            double sum = 0;
            double sumSquares = 0;
            int N = x.Length;
            if(N == 0) { return (0, 0); }

            for(int i = 0; i < N; i++)
            {
                sum += x[i];
                sumSquares += x[i] * x[i];
            }

            double mean = sum / N;
            double std = (sumSquares - sum * sum / N) / (N - 1);
            std = Math.Sqrt(std);

            return (mean, std);
        }

        /// <summary>
        /// Calculates the mean and sample standard deviation of a signal.
        /// </summary>
        public static (double mean, double std) CalcMeanStd(int[] x)
        {
            double sum = 0;
            double sumSquares = 0;
            int N = x.Length;
            if (N == 0) { return (0, 0); }

            for (int i = 0; i < N; i++)
            {
                sum += x[i];
                sumSquares += x[i] * x[i];
            }

            double mean = sum / N;
            double std = (sumSquares - sum * sum / N) / (N - 1);
            std = Math.Sqrt(std);

            return (mean, std);
        }

        /// <summary>
        /// Calculates the histogram of a signal where first bin in histogram corresponds to total count of the min value
        /// and the last bin corresponds to the total count for the max value.
        /// </summary>
        public static (int[] hist, int minVal, int maxVal) CalcHist(int[] x)
        {
            int minVal = x.Min();
            int maxVal = x.Max();

            int bins = maxVal - minVal + 1;
            int[] hist = new int[bins];

            for(int i = 0; i < x.Length; i++)
            {
                hist[x[i] - minVal]++;
            }

            return (hist, minVal, maxVal);
        }

        /// <summary>
        /// Calculates the binned histogram of a signal of doubles. 
        /// </summary>
        public static (int[] hist, double minVal, double maxVal) CalcHist(double[] x, int bins)
        {
            int[] hist = new int[bins];
            double minVal = x.Min();
            double maxVal = x.Max();

            double step = (maxVal - minVal) / bins;
            int bin;

            for(int i = 0; i < x.Length; i++)
            {
                bin = (int)((x[i] - minVal) * step);
                bin = bin >= bins ? bins - 1 : bin;
                hist[bin]++;
            }

            return (hist, minVal, maxVal);
        }

        /// <summary>
        /// Calculates the even decomposition of a signal. x must have an even length.
        /// </summary>
        public static double[] EvenDecompose(double[] x)
        {
            GuardClauses.IsOdd(nameof(EvenDecompose), $"length of {nameof(x)}", x.Length);

            int N = x.Length;
            double[] xE = new double[N];

            xE[0] = x[0];

            for(int i = 1; i < x.Length; i++)
            {
                xE[i] = (x[i] + x[N - i]) / 2;
            }

            return xE;
        }

        /// <summary>
        /// Calculates the odd decomposition of signal. x must have an even length.
        /// </summary>
        public static double[] OddDecompose(double[] x)
        {
            GuardClauses.IsOdd(nameof(OddDecompose), $"length of {nameof(x)}", x.Length);

            int N = x.Length;
            double[] xO = new double[N];

            xO[0] = 0;

            for(int i = 1; i < x.Length; i++)
            {
                xO[i] = (x[i] - x[N - i]) / 2;
            }

            return xO;
        }

        /// <summary>
        /// Decomposes a sequence into it's even and odd samples.
        /// </summary>
        public static (double[] xE, double[] xO) InterlacedDecompose(double[] x)
        {
            GuardClauses.IsOdd(nameof(InterlacedDecompose), $"Length of {nameof(x)}", x.Length);

            double[] xE = new double[x.Length / 2];
            double[] xO = new double[x.Length / 2];

            for (int i = 0; i < x.Length / 2; i++)
            {
                xE[i] = x[2 * i];
                xO[i] = x[2 * i + 1];
            }

            return (xE, xO);
        }

        /// <summary>
        /// Decomposes a sequence into it's even and odd samples.
        /// </summary>
        public static (Complex[] xE, Complex[] xO) InterlacedDecompose(Complex[] x)
        {
            GuardClauses.IsOdd(nameof(InterlacedDecompose), $"Length of {nameof(x)}", x.Length);

            Complex[] xE = new Complex[x.Length / 2];
            Complex[] xO = new Complex[x.Length / 2];

            for (int i = 0; i < x.Length / 2; i++)
            {
                xE[i] = x[2 * i];
                xO[i] = x[2 * i + 1];
            }

            return (xE, xO);
        }

        /// <summary>
        /// Convolves a signal x with kernel h. Output length is length of x plus length of h minus 1.
        /// </summary>
        public static double[] NaiveConv(double[] x, double[] h)
        {
            if(h.Length > x.Length) { return NaiveConv(h, x); }

            int N = x.Length;
            int M = h.Length;
            int L = N + M - 1;
            double[] y = new double[L];

            for(int i = 0; i < L; i++)
            {
                for(int j = 0; j < M; j++)
                {
                    if (i - j < 0) continue;
                    if (i - j >= N) continue;
                    y[i] += h[j] * x[i - j];
                }
            }

            return y;
        }

        /// <summary>
        /// Optimized 1D convolution that avoids boundary checks with each loop. Convolves an input x with a kernel h.
        /// Output length is length of x plus length of h minus 1.
        /// </summary>
        public static double[] OptimConv(double[] x, double[] h)
        {
            if(h.Length > x.Length) { return OptimConv(h, x); }

            int N = x.Length;
            int M = h.Length;
            int L = N + M - 1;
            double[] y = new double[L];

            for(int i = 0; i < M - 1; i++)
            {
                double sum = 0;

                for(int j = 0; j < i + 1; j++)
                {
                    sum += h[j] * x[i - j];
                }

                y[i] = sum;
            }

            for(int i = M - 1; i < L - M + 1; i++)
            {
                double sum = 0;

                for(int j = 0; j < M; j++)
                {
                    sum += h[j] * x[i - j];
                }

                y[i] = sum;
            }

            for(int i = L - M + 1; i < L; i++)
            {
                double sum = 0;

                for(int j = i - L + M; j < M; j++)
                {
                    sum += h[j] * x[i - j];
                }

                y[i] = sum;
            }

            return y;
        }

        /// <summary>
        /// Convolves two sequences and returns an output with the same length of the longest of the two input sequences.
        /// </summary>
        /// <param name="x">Input sequence x</param>
        /// <param name="h">Input sequence h</param>
        /// <returns>Convolved sequence</returns>
        public static double[] TruncConv(double[] x, double[] h)
        {
            if(x.Length < h.Length) { return TruncConv(h, x); }
            int N = x.Length;
            int M = h.Length;
            int MbyTwo = M / 2;
            int L = N + M - 1;
            double[] y = new double[N];

            for (int i = MbyTwo; i < M - 1; i++)
            {
                double sum = 0;

                for (int j = 0; j < i + 1; j++)
                {
                    sum += h[j] * x[i - j];
                }

                y[i - MbyTwo] = sum;
            }

            for (int i = M - 1; i < L - M + 1; i++)
            {
                double sum = 0;

                for (int j = 0; j < M; j++)
                {
                    sum += h[j] * x[i - j];
                }

                y[i - MbyTwo] = sum;
            }

            for (int i = L - M + 1; i < N + MbyTwo; i++)
            {
                double sum = 0;

                for (int j = i - L + M; j < M; j++)
                {
                    sum += h[j] * x[i - j];
                }

                y[i - MbyTwo] = sum;
            }

            return y;
        }

        /// <summary>
        /// Returns a sinc function of length M with cutoff frequency fc. fc must be within 0 and 0.5. M must be odd.
        /// </summary>
        public static double[] Sinc(int M, double fc)
        {
            GuardClauses.IsOutsideLimits(nameof(Sinc), nameof(fc), fc, 0.0, 0.5);
            GuardClauses.IsEven(nameof(Sinc), nameof(M), M);

            int mPrime = M - 1;
            double[] sinc = new double[M];

            double rads = 2 * Math.PI * fc;
            int mByTwo = mPrime / 2;

            for(int i = 0; i < M; i++)
            {
                if(i == mPrime / 2)
                {
                    sinc[i] = 2 * fc;
                    continue;
                }
                int centeredIdx = i - mByTwo;
                sinc[i] = Math.Sin(rads * centeredIdx) / (centeredIdx * Math.PI);
            }

            return sinc;
        }

        /// <summary>
        /// Returns a windowed-sinc sequence of length M with a cutoff frequency of fc. fc must be between 0 and 0.5. Defaults to using
        /// a Blackman window. The returned sequence is normalized so that unity gain is provided at DC.
        /// </summary>
        /// <param name="M">Length of sequence</param>
        /// <param name="fc">cutoff frequency of sequence</param>
        /// <returns>windowed-sinc sequence</returns>
        public static double[] WindowedSinc(int M, double fc)
        {
            GuardClauses.IsEven(nameof(WindowedSinc), nameof(M), M);
            GuardClauses.IsOutsideLimits(nameof(WindowedSinc), nameof(fc), fc, 0, 0.5);

            double[] window = Blackman(M);
            double[] sinc = Sinc(M, fc);
            double sum = 0;

            for(int i = 0; i < sinc.Length; i++)
            {
                sinc[i] *= window[i];
                sum += sinc[i];
            }

            for(int i = 0; i < sinc.Length; i++)
            {
                sinc[i] /= sum;
            }

            return sinc;
        }

        /// <summary>
        /// Generates a Hamming window sequence of total length M
        /// </summary>
        public static double[] Hamming(int M)
        {
            double[] window = new double[M];
            int mPrime = M - 1;

            for(int i = 0; i < M; i++)
            {
                window[i] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / mPrime);
            }

            return window;
        }

        /// <summary>
        /// Generates a Blackman window sequence of total length M
        /// </summary>
        public static double[] Blackman(int M)
        {
            double[] window = new double[M];
            int mPrime = M - 1;

            for(int i = 0; i < M; i++)
            {
                window[i] = 0.42 - 0.5 * Math.Cos(2 * Math.PI * i / mPrime) + 0.08 * Math.Cos(4 * Math.PI * i / mPrime);
            }

            return window;
        }

        /// <summary>
        /// Generates a rectangle sequence of length M where L samples equals the given magnitude starting at the specified
        /// delay point.
        /// </summary>
        public static double[] Rectangle(int M, int L, double magnitude = 1, int delay = 0)
        {
            double[] rectSequence = new double[M];

            int stopIdx = Math.Min(M, delay + L);

            for(int i = delay; i < stopIdx; i++)
            {
                rectSequence[i] = magnitude;
            }

            return rectSequence;
        }

        /// <summary>
        /// Generates an impulse sequence of length M with a given magnitude and delay. All samples are zero except at the given delay timepoint,
        /// where the sequence has a value equal to the given magnitude.
        /// </summary>
        public static double[] Impulse(int M, double magnitude = 1, int delay = 0)
        {
            GuardClauses.IsOutOfBounds(nameof(Impulse), nameof(delay), nameof(M), delay, M);

            double[] impulse = new double[M];
            impulse[delay] = magnitude;
            return impulse;
        }

        /// <summary>
        /// Generates a step sequence of length M where all samples after the given delay are equal to the given magnitude.
        /// </summary>
        public static double[] Step(int M, double magnitude = 1, int delay = 0)
        {
            GuardClauses.IsOutOfBounds(nameof(Step), nameof(delay), nameof(M), delay, M);
            GuardClauses.IsLessThan(nameof(Step), nameof(M), M, 0);

            double[] step = new double[M];

            for(int i = delay; i < M; i++)
            {
                step[i] = magnitude;
            }

            return step;
        }

        /// <summary>
        /// Generates a sin sequence of length M with frequency of k. For example, if k is equal to 2, the sin wave
        /// will complete two cycles within the total sequence length of M
        /// </summary>
        public static double[] SinSequence(int M, int k, double amplitude = 1)
        {
            GuardClauses.IsOutOfBounds(nameof(SinSequence), nameof(k), $"{nameof(M)}/2", k, M / 2);
            double[] sequence = new double[M];

            for(int i = 0; i < M; i++)
            {
                sequence[i] = Math.Sin(2 * Math.PI * k * i / M);
            }

            return sequence;
        }

        /// <summary>
        /// Calculates the moving average of sequence x with given box size. Box size must be odd.
        /// </summary>
        public static double[] AverageFilter(double[] x, int boxSize)
        {
            GuardClauses.IsEven(nameof(AverageFilter), nameof(boxSize), boxSize);
            GuardClauses.IsLessThan(nameof(AverageFilter), nameof(boxSize), boxSize, 0);

            int N = x.Length;
            int halfSize = boxSize / 2;
            int Nprime = x.Length - halfSize;
            double[] y = new double[x.Length];

            for(int i = 0; i < halfSize; i++)
            {
                double sum = 0.0;

                for(int j = 0; j < i + halfSize + 1; j++)
                {
                    sum += x[j];
                }

                y[i] = sum / boxSize;
            }

            for(int i = halfSize; i < Nprime; i++)
            {
                double sum = 0.0;

                for(int j = i - halfSize; j < i + halfSize + 1; j++)
                {
                    sum += x[j];
                }

                y[i] = sum / boxSize;
            }

            for(int i = Nprime; i < N; i++)
            {
                double sum = 0.0;

                for(int j = i - halfSize; j < N; j++)
                {
                    sum += x[j];
                }

                y[i] = sum / boxSize;
            }

            return y;
        }

        /// <summary>
        /// Calculates the magnitude of a given complex sequence.
        /// </summary>
        public static double[] Magnitude(Complex[] x)
        {
            double[] xMag = new double[x.Length];

            for(int i = 0; i < x.Length; i++)
            {
                xMag[i] = x[i].Magnitude;
            }

            return xMag;
        }

        /// <summary>
        /// Calculates the phase of a given complex sequence.
        /// </summary>
        public static double[] Phase(Complex[] x)
        {
            double[] xPhase = new double[x.Length];

            for(int i = 0; i < x.Length; i++)
            {
                xPhase[i] = x[i].Phase;
            }

            return xPhase;
        }

        /// <summary>
        /// Extracts the real part of a complex sequence.
        /// </summary>
        public static double[] Real(Complex[] x)
        {
            double[] re = new double[x.Length];

            for(int i = 0; i < x.Length; i++)
            {
                re[i] = x[i].Real;
            }

            return re;
        }

        /// <summary>
        /// Calculates the decibel value of a given value, where decibel is 20Log10(x).
        /// </summary>
        public static double DB(double x)
        {
            return 20 * Math.Log10(x);
        }

        /// <summary>
        /// Returns a new complex value where the real and imaginary terms are swapped from the original value x.
        /// </summary>
        public static Complex SwapComplex(Complex x)
        {
            return new Complex(x.Imaginary, x.Real);
        }

        /// <summary>
        /// Swaps the real and imaginary components of every complex value in a complex sequence in place.
        /// </summary>
        public static void SwapComplex(ref Complex[] x)
        {
            for(int i = 0; i < x.Length; i++)
            {
                x[i] = SwapComplex(x[i]);
            }
        }

        /// <summary>
        /// Converts a real sequence to a complex sequence whose real values equal the original sequence and all complex 
        /// values equal zero.
        /// </summary>
        public static Complex[] ConvertToComplex(double[] x)
        {
            Complex[] xComplex = new Complex[x.Length];

            for(int i = 0; i < x.Length; i++)
            {
                xComplex[i] = x[i];
            }

            return xComplex;
        }

        /// <summary>
        /// Pads a sequence with zeros so that the returned sequence has a length of newLength.
        /// </summary>
        public static double[] ZeroPad(double[] x, int newLength)
        {
            GuardClauses.IsLessThan(nameof(ZeroPad), nameof(newLength), newLength, x.Length);
            double[] padded = new double[newLength];

            for(int i = 0; i < x.Length; i++)
            {
                padded[i] = x[i];
            }

            return padded;
        }

        /// <summary>
        /// Pads a sequence with zeros so that the returned sequence has a length of newLength.
        /// </summary>
        public static Complex[] ZeroPad(Complex[] x, int newLength)
        {
            GuardClauses.IsLessThan(nameof(ZeroPad), nameof(newLength), newLength, x.Length);
            Complex[] padded = new Complex[newLength];

            for(int i = 0; i < x.Length; i++)
            {
                padded[i] = x[i];
            }

            return padded;
        }

        /// <summary>
        /// Returns the next largest power of two given an unsigned integer x.
        /// </summary>
        public static int NextLargestPowerOfTwo(int x)
        {
            int output = 1;

            for(int i = 0; i < 30; i++)
            {
                if(output >= x) { break; }
                output <<= 1;
            }

            if(output < x)
            {
                throw new OverflowException("Next power of two is outside computational limits.");
            }

            return output;
        }

        /// <summary>
        /// Calculates the Discrete Fourier Transform of a given sequence.
        /// </summary>
        public static Complex[] DFT(double[] x)
        {
            int N = x.Length;
            Complex[] X = new Complex[N];

            for (int k = 0; k < N; k++)
            {
                Complex sum = 0.0;

                for (int n = 0; n < N; n++)
                {
                    sum += x[n] * Complex.Exp(-1 * Complex.ImaginaryOne * 2 * Math.PI * k * n / N);
                }

                X[k] = sum;
            }

            return X;
        }

        /// <summary>
        /// Calculates the inverse Discrete Fourier Transform of a given sequence. Keeps only the real part of the 
        /// conversion, making it only useful for real signals and not complex signals.
        /// </summary>
        public static double[] IDFT(Complex[] x)
        {
            int N = x.Length;
            double[] X = new double[N];

            for (int n = 0; n < N; n++)
            {
                Complex sum = 0.0;

                for (int k = 0; k < N; k++)
                {
                    sum += x[k] * Complex.Exp(Complex.ImaginaryOne * 2 * Math.PI * k * n / N);
                }

                X[n] = sum.Real / N;
            }

            return X;
        }

        /// <summary>
        /// Performs a Fast Fourier Transform on an input sequence x
        /// </summary>
        public static Complex[] FFT(Complex[] x)
        {
            int powTwo = NextLargestPowerOfTwo(x.Length);
            if(powTwo != x.Length)
            {
                x = ZeroPad(x, powTwo);
            }

            Complex[] f = DitFft(x);

            return f;
        }

        /// <summary>
        /// Performs a Fast Fourier Transform on an input sequence x
        /// </summary>
        public static Complex[] FFT(double[] x)
        {
            Complex[] xComplex = ConvertToComplex(x);
            return FFT(xComplex);
        }

        /// <summary>
        /// Performs a decimation in time Fast Fourier Transform as described by the Cooley-Tukey algorithm.
        /// Assumes the input has a length that is a power of two.
        /// </summary>
        private static Complex[] DitFft(Complex[] x)
        {
            int N = x.Length;
            Complex[] xOut = new Complex[N];

            if (N == 1) { xOut[0] = x[0]; }
            else
            {
                Complex[] xE, xO;
                (xE, xO) = InterlacedDecompose(x);
                xE = DitFft(xE);
                xO = DitFft(xO);

                for (int k = 0; k < N / 2; k++)
                {
                    Complex p = xE[k];
                    Complex q = Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * k / N) * xO[k];
                    xOut[k] = p + q;
                    xOut[k + N / 2] = p - q;
                }
            }

            return xOut;
        }

        /// <summary>
        /// Computes the inverse Fast Fourier Transform of a complex sequence x.
        /// </summary>
        public static Complex[] IFFT(Complex[] x)
        {
            Complex[] xInverse = new Complex[x.Length];
            Array.Copy(x, xInverse, x.Length);

            SwapComplex(ref xInverse);
            xInverse = FFT(xInverse);
            SwapComplex(ref xInverse);

            for(int i = 0; i < xInverse.Length; i++)
            {
                xInverse[i] /= xInverse.Length;
            }

            return xInverse;
        }

        /// <summary>
        /// Inverts a filter kernel so that the magnitude of the Fourier Transform of the kernel is flipped. For example, 
        /// a lowpass kernel will become a highpass kernel with the same cutoff frequency.
        /// </summary>
        public static double[] SpectralInversion(double[] kernel)
        {
            GuardClauses.IsEven(nameof(SpectralInversion), $"{nameof(kernel)} length", kernel.Length);

            double[] inverted = new double[kernel.Length];
            Array.Copy(kernel, inverted, kernel.Length);

            for(int i = 0; i < inverted.Length; i++)
            {
                inverted[i] = -inverted[i];
            }

            inverted[inverted.Length / 2] += 1;

            return inverted;
        }

        /// <summary>
        /// Spectrally reverses a filter kernel so that the magnitude of the Fourier Transform is flipped around the discrete frequency
        /// of 0.25. For example, a lowpass filter with a cutoff frequency of 0.2 will become a highpass filter with a cutoff frequency 
        /// of 0.3. (Discrete frequencies range from 0 to 0.5)
        /// </summary>
        public static double[] SpectralReversal(double[] kernel)
        {
            GuardClauses.IsEven(nameof(SpectralReversal), $"{nameof(kernel)} length", kernel.Length);

            double[] reversed = new double[kernel.Length];
            Array.Copy(kernel, reversed, kernel.Length);

            for (int i = 1; i < reversed.Length; i += 2)
            {
                reversed[i] *= -1;
            }

            return reversed;
        }
    }
}
