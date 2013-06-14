namespace SoundFingerprinting.FFT.Exocortex
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    // Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
    // Version: May 4, 2002

    /// <summary>
    ///   <p>A set of array utilities for complex number arrays</p>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1404:CodeAnalysisSuppressionMustHaveJustification", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1404:CodeAnalysisSuppressionMustHaveJustification", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:SingleLineCommentsMustBeginWithSingleSpace", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1512:SingleLineCommentsMustNotBeFollowedByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:StaticReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1027:TabsMustNotBeUsed")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingCurlyBracketMustBeFollowedByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1405:DebugAssertMustProvideMessageText", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:StatementMustNotUseUnnecessaryParenthesis", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1003:SymbolsMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:CommasMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:OpeningParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText",
        Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1616:ElementReturnValueDocumentationMustHaveText", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:StaticElementsMustAppearBeforeInstanceElements", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "Reviewed. Suppression is OK here.")]
    internal class ComplexArray
    {
        //---------------------------------------------------------------------------------------------

        private static bool _workspaceFLocked;
        private static ComplexF[] _workspaceF = new ComplexF[0];

        private ComplexArray()
        {
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Clamp length (modulus) of the elements in the complex array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "fMinimum"></param>
        /// <param name = "fMaximum"></param>
        public static void ClampLength(Complex[] array, double fMinimum, double fMaximum)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Complex.FromModulusArgument(Math.Max(fMinimum, Math.Min(fMaximum, array[i].GetModulus())), array[i].GetArgument());
            }
        }

        /// <summary>
        ///   Clamp elements in the complex array to range [minimum,maximum]
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "minimum"></param>
        /// <param name = "maximum"></param>
        public static void Clamp(Complex[] array, Complex minimum, Complex maximum)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Re = Math.Min(Math.Max((sbyte) array[i].Re, (sbyte) minimum.Re), (sbyte) maximum.Re);
                array[i].Im = Math.Min(Math.Max((sbyte) array[i].Re, (sbyte) minimum.Im), (sbyte) maximum.Im);
            }
        }

        /// <summary>
        ///   Clamp elements in the complex array to real unit range (i.e. [0,1])
        /// </summary>
        /// <param name = "array"></param>
        public static void ClampToRealUnit(Complex[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Re = Math.Min(Math.Max(array[i].Re, 0), 1);
                array[i].Im = 0;
            }
        }

        //---------------------------------------------------------------------------------------------

// ReSharper disable RedundantAssignment
        private static void LockWorkspaceF(int length, ref ComplexF[] workspace)
// ReSharper restore RedundantAssignment
        {
            Debug.Assert(_workspaceFLocked == false);
            _workspaceFLocked = true;
            if (length >= _workspaceF.Length)
            {
                _workspaceF = new ComplexF[length];
            }
            workspace = _workspaceF;
        }

        private static void UnlockWorkspaceF(ref ComplexF[] workspace)
        {
            Debug.Assert(_workspaceF == workspace);
            Debug.Assert(_workspaceFLocked);
            _workspaceFLocked = false;
            workspace = null;
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Shift (offset) the elements in the array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "offset"></param>
        public static void Shift(Complex[] array, int offset)
        {
            Debug.Assert(array != null);
            Debug.Assert(offset >= 0);
            Debug.Assert(offset < array.Length);

            if (offset == 0)
            {
                return;
            }

            int length = array.Length;
            Complex[] temp = new Complex[length];

            for (int i = 0; i < length; i++)
            {
                temp[(i + offset)%length] = array[i];
            }
            for (int i = 0; i < length; i++)
            {
                array[i] = temp[i];
            }
        }

        /// <summary>
        ///   Shift (offset) the elements in the array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "offset"></param>
        public static void Shift(ComplexF[] array, int offset)
        {
            Debug.Assert(array != null);
            Debug.Assert(offset >= 0);
            Debug.Assert(offset < array.Length);

            if (offset == 0)
            {
                return;
            }

            int length = array.Length;
            ComplexF[] workspace = null;
            LockWorkspaceF(length, ref workspace);

            for (int i = 0; i < length; i++)
            {
                workspace[(i + offset)%length] = array[i];
            }
            for (int i = 0; i < length; i++)
            {
                array[i] = workspace[i];
            }

            UnlockWorkspaceF(ref workspace);
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Get the range of element lengths
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "minimum"></param>
        /// <param name = "maximum"></param>
// ReSharper disable RedundantAssignment
        public static void GetLengthRange(Complex[] array, ref double minimum, ref double maximum)
// ReSharper restore RedundantAssignment
        {
            minimum = +double.MaxValue;
            maximum = -double.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                double temp = array[i].GetModulus();
                minimum = Math.Min(temp, minimum);
                maximum = Math.Max(temp, maximum);
            }
        }

        /// <summary>
        ///   Get the range of element lengths
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "minimum"></param>
        /// <param name = "maximum"></param>
// ReSharper disable RedundantAssignment
        public static void GetLengthRange(ComplexF[] array, ref float minimum, ref float maximum)
// ReSharper restore RedundantAssignment
        {
            minimum = +float.MaxValue;
            maximum = -float.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                float temp = array[i].GetModulus();
                minimum = Math.Min(temp, minimum);
                maximum = Math.Max(temp, maximum);
            }
        }

        // // <summary>
        // // Conver the complex array to a double array
        // // </summary>
        // // <param name="array"></param>
        // // <param name="style"></param>
        // // <returns></returns>
        /* static public double[]	ConvertToDoubleArray( Complex[] array, ConversionStyle style ) {
            double[] newArray = new double[ array.Length ];
            switch( style ) {
            case ConversionStyle.Length:
                for( int i = 0; i < array.Length; i ++ ) {
                    newArray[i] = (double) array[i].GetModulus();
                }
                break;
            case ConversionStyle.Real:
                for( int i = 0; i < array.Length; i ++ ) {
                    newArray[i] = (double) array[i].Re;
                }
                break;
            case ConversionStyle.Imaginary:
                for( int i = 0; i < array.Length; i ++ ) {
                    newArray[i] = (double) array[i].Im;
                }
                break;
            default:
                Debug.Assert( false );
                break;
            }
            return	newArray;
        }	 */

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Determine whether the elements in the two arrays are the same
        /// </summary>
        /// <param name = "array1"></param>
        /// <param name = "array2"></param>
        /// <param name = "tolerance"></param>
        /// <returns></returns>
        public static bool IsEqual(Complex[] array1, Complex[] array2, double tolerance)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; i++)
            {
                if (Complex.IsEqual(array1[i], array2[i], tolerance) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///   Determine whether the elements in the two arrays are the same
        /// </summary>
        /// <param name = "array1"></param>
        /// <param name = "array2"></param>
        /// <param name = "tolerance"></param>
        /// <returns></returns>
        public static bool IsEqual(ComplexF[] array1, ComplexF[] array2, float tolerance)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; i++)
            {
                if (ComplexF.IsEqual(array1[i], array2[i], tolerance) == false)
                {
                    return false;
                }
            }
            return true;
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Add a specific value to each element in the array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "offset"></param>
        public static void Offset(Complex[] array, double offset)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i].Re += offset;
            }
        }

        /// <summary>
        ///   Add a specific value to each element in the array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "offset"></param>
        public static void Offset(Complex[] array, Complex offset)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i] += offset;
            }
        }

        /// <summary>
        ///   Add a specific value to each element in the array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "offset"></param>
        public static void Offset(ComplexF[] array, float offset)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i].Re += offset;
            }
        }

        /// <summary>
        ///   Add a specific value to each element in the array
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "offset"></param>
        public static void Offset(ComplexF[] array, ComplexF offset)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i] += offset;
            }
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        public static void Scale(Complex[] array, double scale)
        {
            Debug.Assert(array != null);

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        /// <param name = "start"></param>
        /// <param name = "length"></param>
        public static void Scale(Complex[] array, double scale, int start, int length)
        {
            Debug.Assert(array != null);
            Debug.Assert(start >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert((start + length) < array.Length);

            for (int i = 0; i < length; i++)
            {
                array[i + start] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        public static void Scale(Complex[] array, Complex scale)
        {
            Debug.Assert(array != null);

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        /// <param name = "start"></param>
        /// <param name = "length"></param>
        public static void Scale(Complex[] array, Complex scale, int start, int length)
        {
            Debug.Assert(array != null);
            Debug.Assert(start >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert((start + length) < array.Length);

            for (int i = 0; i < length; i++)
            {
                array[i + start] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        public static void Scale(ComplexF[] array, float scale)
        {
            Debug.Assert(array != null);

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        /// <param name = "start"></param>
        /// <param name = "length"></param>
        public static void Scale(ComplexF[] array, float scale, int start, int length)
        {
            Debug.Assert(array != null);
            Debug.Assert(start >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert((start + length) < array.Length);

            for (int i = 0; i < length; i++)
            {
                array[i + start] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        public static void Scale(ComplexF[] array, ComplexF scale)
        {
            Debug.Assert(array != null);

            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                array[i] *= scale;
            }
        }

        /// <summary>
        ///   Multiply each element in the array by a specific value
        /// </summary>
        /// <param name = "array"></param>
        /// <param name = "scale"></param>
        /// <param name = "start"></param>
        /// <param name = "length"></param>
        public static void Scale(ComplexF[] array, ComplexF scale, int start, int length)
        {
            Debug.Assert(array != null);
            Debug.Assert(start >= 0);
            Debug.Assert(length >= 0);
            Debug.Assert((start + length) < array.Length);

            for (int i = 0; i < length; i++)
            {
                array[i + start] *= scale;
            }
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Multiply each element in target array with corresponding element in rhs array
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "rhs"></param>
        public static void Multiply(Complex[] target, Complex[] rhs)
        {
            Multiply(target, rhs, target);
        }

        /// <summary>
        ///   Multiply each element in lhs array with corresponding element in rhs array and
        ///   put product in result array
        /// </summary>
        /// <param name = "lhs"></param>
        /// <param name = "rhs"></param>
        /// <param name = "result"></param>
        public static void Multiply(Complex[] lhs, Complex[] rhs, Complex[] result)
        {
            Debug.Assert(lhs != null);
            Debug.Assert(rhs != null);
            Debug.Assert(result != null);
            Debug.Assert(lhs.Length == rhs.Length);
            Debug.Assert(lhs.Length == result.Length);

            int length = lhs.Length;
            for (int i = 0; i < length; i++)
            {
                result[i] = lhs[i] * rhs[i];
            }
        }

        /// <summary>
        ///   Multiply each element in target array with corresponding element in rhs array
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "rhs"></param>
        public static void Multiply(ComplexF[] target, ComplexF[] rhs)
        {
            Multiply(target, rhs, target);
        }

        /// <summary>
        ///   Multiply each element in lhs array with corresponding element in rhs array and
        ///   put product in result array
        /// </summary>
        /// <param name = "lhs"></param>
        /// <param name = "rhs"></param>
        /// <param name = "result"></param>
        public static void Multiply(ComplexF[] lhs, ComplexF[] rhs, ComplexF[] result)
        {
            Debug.Assert(lhs != null);
            Debug.Assert(rhs != null);
            Debug.Assert(result != null);
            Debug.Assert(lhs.Length == rhs.Length);
            Debug.Assert(lhs.Length == result.Length);

            int length = lhs.Length;
            for (int i = 0; i < length; i++)
            {
                result[i] = lhs[i] * rhs[i];
            }
        }

        //---------------------------------------------------------------------------------------------

        /// <summary>
        ///   Divide each element in target array with corresponding element in rhs array
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "rhs"></param>
        public static void Divide(Complex[] target, Complex[] rhs)
        {
            Divide(target, rhs, target);
        }

        /// <summary>
        ///   Divide each element in lhs array with corresponding element in rhs array and
        ///   put product in result array
        /// </summary>
        /// <param name = "lhs"></param>
        /// <param name = "rhs"></param>
        /// <param name = "result"></param>
        public static void Divide(Complex[] lhs, Complex[] rhs, Complex[] result)
        {
            Debug.Assert(lhs != null);
            Debug.Assert(rhs != null);
            Debug.Assert(result != null);
            Debug.Assert(lhs.Length == rhs.Length);
            Debug.Assert(lhs.Length == result.Length);

            int length = lhs.Length;
            for (int i = 0; i < length; i++)
            {
                result[i] = lhs[i] / rhs[i];
            }
        }

        /// <summary>
        ///   Divide each element in target array with corresponding element in rhs array
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "rhs"></param>
        public static void Divide(ComplexF[] target, ComplexF[] rhs)
        {
            Divide(target, rhs, target);
        }

        /// <summary>
        ///   Divide each element in lhs array with corresponding element in rhs array and
        ///   put product in result array
        /// </summary>
        /// <param name = "lhs"></param>
        /// <param name = "rhs"></param>
        /// <param name = "result"></param>
        public static void Divide(ComplexF[] lhs, ComplexF[] rhs, ComplexF[] result)
        {
            Debug.Assert(lhs != null);
            Debug.Assert(rhs != null);
            Debug.Assert(result != null);
            Debug.Assert(lhs.Length == rhs.Length);
            Debug.Assert(lhs.Length == result.Length);

            ComplexF zero = ComplexF.Zero;
            int length = lhs.Length;
            for (int i = 0; i < length; i++)
            {
                if (rhs[i] != zero)
                {
                    result[i] = lhs[i] / rhs[i];
                }
                else
                {
                    result[i] = zero;
                }
            }
        }

        //---------------------------------------------------------------------------------------------

        /*static public void Flip( ComplexF[] array, Size3 size ) {
            Debug.Assert( array != null );

            ComplexF[]	workspace	= null;
            ComplexArray.LockWorkspaceF( size.GetTotalLength(), ref workspace );
			
            for( int z = 0; z < size.Depth; z ++ ) {
                for( int y = 0; y < size.Height; y ++ ) {
                    int xyzOffset = 0 + y * size.Width + z * size.Width * size.Height;
                    int abcOffset = size.Width - 1 + ( size.Height - y - 1 ) * size.Width + ( size.Depth - z - 1 ) * size.Width * size.Height;
                    for( int x = 0; x < size.Width; x ++ ) {
                        workspace[ xyzOffset ++ ] = array[ abcOffset -- ];
                    }
                }
            }

            for( int i = 0; i < size.GetTotalLength(); i ++ ) {
                array[ i ] = workspace[ i ];
            }

            ComplexArray.UnlockWorkspaceF( ref workspace );
        }  */

        /// <summary>
        ///   Copy an array
        /// </summary>
        /// <param name = "dest"></param>
        /// <param name = "source"></param>
        public static void Copy(Complex[] dest, Complex[] source)
        {
            Debug.Assert(dest != null);
            Debug.Assert(source != null);
            Debug.Assert(dest.Length == source.Length);
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = source[i];
            }
        }

        /// <summary>
        ///   Copy an array
        /// </summary>
        /// <param name = "dest"></param>
        /// <param name = "source"></param>
        public static void Copy(ComplexF[] dest, ComplexF[] source)
        {
            Debug.Assert(dest != null);
            Debug.Assert(source != null);
            Debug.Assert(dest.Length == source.Length);
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = source[i];
            }
        }

        /// <summary>
        ///   Reverse the elements in the array
        /// </summary>
        /// <param name = "array"></param>
        public static void Reverse(Complex[] array)
        {
            Complex temp;
            int length = array.Length;
            for (int i = 0; i < length/2; i++)
            {
                temp = array[i];
                array[i] = array[length - 1 - i];
                array[length - 1 - i] = temp;
            }
        }

        /// <summary>
        ///   Scale and offset the elements in the array so that the
        ///   overall range is [0, 1]
        /// </summary>
        /// <param name = "array"></param>
        public static void Normalize(Complex[] array)
        {
            double min = 0, max = 0;
            GetLengthRange(array, ref min, ref max);
            Scale(array, (1/(max - min)));
            Offset(array, (-min/(max - min)));
        }

        /// <summary>
        ///   Scale and offset the elements in the array so that the
        ///   overall range is [0, 1]
        /// </summary>
        /// <param name = "array"></param>
        public static void Normalize(ComplexF[] array)
        {
            float min = 0, max = 0;
            GetLengthRange(array, ref min, ref max);
            Scale(array, (1/(max - min)));
            Offset(array, (-min/(max - min)));
        }

        /// <summary>
        ///   Invert each element in the array
        /// </summary>
        /// <param name = "array"></param>
        public static void Invert(Complex[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ((Complex) 1)/array[i];
            }
        }

        /// <summary>
        ///   Invert each element in the array
        /// </summary>
        /// <param name = "array"></param>
        public static void Invert(ComplexF[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ((ComplexF) 1)/array[i];
            }
        }

        //----------------------------------------------------------------------------------------
    }
}