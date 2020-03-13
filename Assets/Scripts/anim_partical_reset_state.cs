using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class anim_partical_reset_state : MonoBehaviour
{

    ParticleSystem[] m_particleSystem;
    Animation[] m_animation;
    private Animator[] m_animator;
    bool[] m_particleInitShow;
    bool[] m_particleShow;
    float delTime = 0;
    bool run = true;
    float preParticle = 0f;
    bool[] m_preParticleShow;
    void Awake() 
    {
        HandleAwake();
	}
    public void HandleAwake()
    {
        m_particleSystem = GetComponentsInChildren<ParticleSystem>(true);
        m_animation = GetComponentsInChildren<Animation>(true);
        m_animator = GetComponentsInChildren<Animator>();
        m_particleInitShow = new bool[m_particleSystem.Length];
        for (int i = 0; i < m_particleSystem.Length; ++i)
        {
            m_particleInitShow[i] = m_particleSystem[i].gameObject.activeInHierarchy;
//            m_particleSystem[i].playbackSpeed = 0f;
//            m_particleInitShow[i]
        }
        
        for (int i = 0; i < m_animator.Length; ++i)
        {
            m_animator[i].speed = 0;
        }
        m_particleShow = new bool[m_particleSystem.Length];
        m_preParticleShow = new bool[m_particleSystem.Length];
        delTime = 0;
    }

    public void ResetState()
    {
        for (int i = 0; i < m_particleSystem.Length; ++i)
        {
            m_particleSystem[i].gameObject.SetActive(m_particleInitShow[i]);
            m_particleShow[i] = false;
        }
        delTime = 0;
        run = true;
    }
    
	void OnEnable()
    {
        ResetState();
    }
    
    public void SetRun(bool b)
    {
        run = b;
        if(!run)
        {
            for (int i = 0; i < m_particleSystem.Length; ++i)
            {
                m_particleSystem[i].Clear();
                m_particleSystem[i].Stop();
            }
        }
    }
	// Update is called once per frame
	public void ParticleUpdate(float dt) 
    {
        if (!run)
            return;
        delTime = delTime + dt;
        UpdateParticle(delTime);
	}
    public void UpdateParticle(float time)
    {
        delTime = time;
        for (int i = 0; i < m_animation.Length; ++i)
        {
            m_animation[i].clip.SampleAnimation(m_animation[i].gameObject, delTime);
        }
        for (int i = 0; i < m_animator.Length; ++i)
        {
            if (m_animator[i].runtimeAnimatorController != null)
            {
                AnimatorStateInfo stateInfo = m_animator[i].GetCurrentAnimatorStateInfo(0);
                m_animator[i].Play(stateInfo.nameHash, 0, delTime);
            }
        }
        for (int i = 0; i < m_particleSystem.Length; ++i)
        {
            if (m_particleSystem[i].gameObject.activeInHierarchy)
            {
                if (!m_particleShow[i])
                {
                    m_particleSystem[i].Simulate(0, false, true);
                    m_particleShow[i] = true;
                }
                if (delTime < preParticle)
                {
                    m_particleSystem[i].Simulate(0, false, true);
                    m_particleSystem[i].Simulate(delTime, false, false);
                }
                else
                {
                    m_particleSystem[i].Simulate(delTime - preParticle, false, false);
                }
                m_preParticleShow[i] = true;
            }
            else
            {
                if(m_preParticleShow[i])
                {
                    m_particleShow[i] = false;
                }
                m_preParticleShow[i] = false;
            }
        }
        preParticle = delTime;
    }
}
