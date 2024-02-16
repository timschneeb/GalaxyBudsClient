using System;

namespace GalaxyBudsClient.Utils
{
    public class BoyerMoore
    {
        private int[] _jumpTable = Array.Empty<int>();
        private byte[] _pattern = Array.Empty<byte>();
        private int _patternLength = 0;

        public void SetPattern(byte[] pattern)
        {
            _pattern = pattern;
            _jumpTable = new int[256];
            _patternLength = _pattern.Length;
            for(var index = 0; index < 256; index++)
                _jumpTable[index] = _patternLength;
            for(var index = 0; index < _patternLength - 1; index++)
                _jumpTable[_pattern[index]] = _patternLength - index - 1;
        }
        
        public unsafe int Search(byte[] searchArray, int startIndex = 0)
        {
            if(_patternLength > searchArray.Length)
                throw new Exception("Search Pattern length exceeds search array length.");
            
            var index = startIndex;
            var limit= searchArray.Length - _patternLength;
            var patternLengthMinusOne = _patternLength     - 1;
            fixed(byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed(byte* pointerToPattern = _pattern)
                {
                    while(index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while(j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                            j--;
                        if(j < 0)
                            return index;
                        index += Math.Max(_jumpTable[pointerToByteArrayStartingIndex[index + j]] - _patternLength + 1 + j, 1);
                    }
                }
            }
            return -1;
        }
    }
}