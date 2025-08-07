using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class NumberFormatter
{
    // 사용할 숫자 단위 접미사 리스트
    private static readonly List<string> suffixes = new List<string> 
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ"};

    // 접미사를 제곱 지수로 매핑하는 딕셔너리
    private static readonly Dictionary<string, int> suffixToPower = new Dictionary<string, int>();

    // 정적 생성자에서 접미사 매핑 초기화
    static NumberFormatter()
    {
        for (int i = 0; i < suffixes.Count; i++)
        {
            suffixToPower[suffixes[i]] = (i + 1) * 3; // A -> 10^3, B -> 10^6, ...
        }
    }

    /// <summary>
    /// BigInteger 타입의 매우 큰 숫자를 A, B, C 단위의 축약된 문자열로 변환
    /// </summary>
    /// <param name="number">변환할 숫자 (BigInteger)</param>
    /// <returns>포맷팅된 문자열 (예: 1.2A, 150.5B, 32.7C)</returns>

    public static string FormatNumber(double number)
    {
        if (number < 1000f)
        {
            return number.ToString("F2");
        }
    
        int unitIndex = 0;
        double tempNumber = number;

        //1000으로 나눠가며 단위 인덱스 찾기
        while (tempNumber >= 1000f && unitIndex < suffixes.Count)
        {
            tempNumber /= 1000f;
            unitIndex++;
        }
        
        //단위 범위를 초과할 경우 마지막 단위 사용
        if (unitIndex >= suffixes.Count)
        {
            Debug.Log("숫자가 너무 커서 정의된 단위를 넘어섰습니다.");
            unitIndex = suffixes.Count - 1;
        }
        
        //최종 표시값 계산
        double displayValue = number / Mathf.Pow(1000, unitIndex);
        string suffix = unitIndex > 0 ? suffixes[unitIndex - 1] : "";
        return displayValue.ToString("F1") + suffix;
    }

    public static string FormatNumber(BigInteger number)
    {
        if (number < 1000)
            return number.ToString();
        
        int unitIndex = 0;
        BigInteger divisor = 1000;
        
        //단위 인덱스 계산
        while (number / divisor >= 1000 && unitIndex < suffixes.Count - 1)
        {
            divisor *= 1000;
            unitIndex++;
        }
        
        //정수부 계산
        BigInteger whole = number / divisor;
        
        //소수점 첫째 자리 계산
        BigInteger remainder = (number % divisor) * 10 / divisor;
        
        //문자열로 조합
        return $"{whole}.{remainder}{suffixes[unitIndex]}";
    }
    
    // public static string FormatNumber(BigInteger number)
    // {
    //     // 1000 미만은 그대로 표시
    //     if (number < 1000)
    //     {
    //         return number.ToString();
    //     }
    //
    //     // 숫자를 1000으로 몇 번 나눴는지(단위 인덱스)를 계산할 변수
    //     int unitIndex = 0;
    //     // 계산을 위한 임시 변수
    //     BigInteger tempNumber = number;
    //     
    //     // 숫자가 1000 이상인 동안 계속 1000으로 나누며 단위를 올림
    //     while (tempNumber >= 1000)
    //     {
    //         tempNumber /= 1000;
    //         unitIndex++;
    //     }
    //
    //     // 실제 사용할 접미사 인덱스 (A부터 시작하므로 -1)
    //     int suffixIndex = unitIndex - 1;
    //
    //     // 만약 정의된 접미사 범위를 넘어서면 가장 큰 단위를 사용
    //     if (suffixIndex >= suffixes.Count)
    //     {
    //         Debug.LogWarning("숫자가 너무 커서 정의된 단위를 넘어섰습니다.");
    //         suffixIndex = suffixes.Count - 1;
    //     }
    //
    //     // ★★★ 핵심: 최종 표시를 위해 BigInteger를 double로 변환하여 소수점 표현 ★★★
    //     // 예: 1,523,456 -> 1523.456 -> 1.523...
    //     double displayValue = (double)number / Math.Pow(1000, unitIndex);
    //     
    //     // 소수점 첫째 자리까지 표시하고, 단위 접미사를 붙임
    //     return displayValue.ToString("F1") + suffixes[suffixIndex];
    // }

    /// <summary>
    /// "1.5A", "12.3B" 같은 단위 문자열을 BigInteger로 역변환
    /// </summary>
    /// <param name="str">입력 문자열 (예: "1.5A")</param>
    /// <returns>변환된 BigInteger 숫자</returns>
    public static BigInteger Parse(string str)
    {
        str = str.ToUpper().Trim();

        foreach (var kvp in suffixToPower)
        {
            if (str.EndsWith(kvp.Key))
            {
                string numberPart = str.Substring(0, str.Length - kvp.Key.Length);
                if (double.TryParse(numberPart, out double baseValue))
                {
                    return new BigInteger(baseValue * Math.Pow(10, kvp.Value));
                }
            }
        }

        return BigInteger.TryParse(str, out var result) ? result : BigInteger.Zero;
    }
}
