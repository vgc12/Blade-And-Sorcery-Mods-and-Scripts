﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace Marai
{
    public class MaraiOrbFX : MonoBehaviour
    {
        //Item Setup
        protected Item item;
        protected ItemModuleMaraiOrb module;
        private MeshRenderer orb, firstRune, secondRune;
        private Transform runeTarget;

        //LerpOrbAlpha
        private float minimumOrbAlpha = .02F;
        private float maximumOrbAlpha = .65F;
        private float t = 0.0f;
        

        public float scrollSpeed = .5f;
        
        private void Awake()
        {
           
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleMaraiOrb>();
          

            orb = item.GetCustomReference(module.OrbRef).GetComponent<MeshRenderer>();
          
            firstRune = item.GetCustomReference(module.FirstRuneRef).GetComponent<MeshRenderer>();
          
            secondRune = item.GetCustomReference(module.SecondRuneRef).GetComponent<MeshRenderer>();
           
            runeTarget = item.GetCustomReference(module.RuneTarget).GetComponent<Transform>();
            

        }

        private void Update()
        {
            LerpOrbAlpha();
            MoveOrbTexOffset();
            RotateRuneToTarget();
            SpinSecondRune();
        }

        private void MoveOrbTexOffset()
        {
            float offset = scrollSpeed * Time.time;
            orb.material.SetTextureOffset("_BaseMap", new Vector2(offset, offset * 1.2f));
        }

        private void RotateRuneToTarget()
        {
            //fix this
            /*
            Vector3 rotation = Quaternion.LookRotation(runeTarget.position).eulerAngles;

            rotation.x = 0f;
            firstRune.transform.rotation = Quaternion.Euler(rotation);
            */
            firstRune.transform.LookAt(runeTarget.transform, runeTarget.transform.up);
        }

        private void SpinSecondRune()
        {
            secondRune.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, Time.time * 50));
        }

        private void LerpOrbAlpha()
        {
            {
                

                orb.material.SetFloat("_Cutoff", Mathf.Lerp(minimumOrbAlpha, maximumOrbAlpha, t));
                t += 0.5f * Time.deltaTime;

                if (t > 1.0f)
                {
                    float temp = maximumOrbAlpha;
                    maximumOrbAlpha = minimumOrbAlpha;
                    minimumOrbAlpha = temp;
                    t = 0.0f;
                }
            }

        }
    }
}
