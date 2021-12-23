using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillSet
{
    [SerializeField]
    private List<Skill> skills = new List<Skill>(); // Unity hates Sets apperantly

    public void AddSkill(Skill skill)
    {
        skills.Add(skill);
    }

    public bool HasSkill(Skill skill)
    {
        return skills.Contains(skill);
    }
}
