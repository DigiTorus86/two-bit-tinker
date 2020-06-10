using System;
using System.Diagnostics;
using System.Numerics;  // For Complex

namespace uwave
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class Dft
	{
		/// <summary>
		/// Computes the discrete Fourier transform (DFT) of the given WAV audio sample.
		/// </summary>
		/// <param name="wav"></param>
		/// <param name="frameSize"></param>
		/// <returns></returns>
		public static Complex[] CalculateDFT(WavAudio wav, UInt32 frameSize)
		{
			Complex[] input = new Complex[frameSize];  

			// Windowing and conversion to Complex 
			for (UInt16 i = 0; i < frameSize; i++)
			{
				double window = -.5 * Math.Cos(2.0 * Math.PI * (double)i / (double)frameSize) + .5;
				input[i] = new Complex((float)((wav.GetSample(i, 0) - wav.Silence) * window), 0f);
			}

			return CalculateDFT(input);
		}

		/// <summary>
		/// Computes the discrete Fourier transform (DFT) of the given complex vector.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Complex[] CalculateDFT(Complex[] input)
		{
			int n = input.Length;
			Complex[] output = new Complex[n];
			for (int k = 0; k < n; k++)
			{  // For each output element
				Complex sum = 0;
				for (int t = 0; t < n; t++)
				{  // For each input element
					double angle = 2 * Math.PI * t * k / n;
					sum += input[t] * Complex.Exp(new Complex(0, -angle));
				}
				output[k] = sum;
			}
			return output;
		}


		/// <summary>
		/// Computes the discrete Fourier transform (DFT) of the given complex vector.
		/// All the array arguments must be non-null and have the same length.
		/// </summary>
		/// <param name="inReal"></param>
		/// <param name="inImag"></param>
		/// <param name="outReal"></param>
		/// <param name="outImag"></param>
		public static void CalculateDFT(double[] inReal, double[] inImag, double[] outReal, double[] outImag)
		{
			int n = inReal.Length;
			for (int k = 0; k < n; k++)
			{  // For each output element
				double sumReal = 0;
				double sumImag = 0;
				for (int t = 0; t < n; t++)
				{  // For each input element
					double angle = 2 * Math.PI * t * k / n;
					sumReal += inReal[t] * Math.Cos(angle) + inImag[t] * Math.Sin(angle);
					sumImag += -inReal[t] * Math.Sin(angle) + inImag[t] * Math.Cos(angle);
				}
				outReal[k] = sumReal;
				outImag[k] = sumImag;
			}
		}

		public static double FindMaxReal(Complex[] values)
		{
			double maxValue = 0;

			for (int i = 0; i < values.Length / 2; i++)
			{
				if (values[i].Real > maxValue)
					maxValue = values[i].Real;
			}
			return maxValue;
		}

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}