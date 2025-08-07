using System;
using Data;
using System.Data;
using System.Text.RegularExpressions;

public static class SkillLevelCalculator
{
    // 계산식에 baseValue와 level을 넣어서 계산해주는 함수
    public static float CalculateMultiplier(int skillDataId, int skillLevel)
    {
        if (!Managers.Data.SkillLevelDataDic.TryGetValue(skillDataId, out SkillLevelData levelData))
        {
            return 1f;
        }

        string formula = levelData.formula; 
        float baseValue = levelData.baseValue;

        formula = formula.Replace("baseValue", baseValue.ToString());
        formula = formula.Replace("level", skillLevel.ToString());

        try
        {
            var result = new DataTable().Compute(formula, null);
            return Convert.ToSingle(result);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"SkillLevelCalculator 계산 오류: {e.Message}");
            return 1f;
        }
    }
}