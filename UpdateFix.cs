using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine.Events;
using System;

namespace InputFix{

    [HarmonyPatch(typeof(GameController))]    
    public class Patches{
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool UpdateFix(GameController __instance)
        {
            if (__instance.multtexthide > -1f)
            {
                __instance.multtexthide += 1f * Time.deltaTime;
                if (__instance.multtexthide > 1.5f)
                {
                    __instance.multtexthide = -1f;
                    __instance.hideMultText();
                }
            }
            float x = __instance.healthfill.transform.localPosition.x;
            float num4 = (-4.4f + (__instance.currenthealth / 100f) * 3.68f - x) * (6.85f * Time.deltaTime);
            __instance.healthposy += 13.5f * Time.deltaTime;
            if (__instance.healthposy > 3.08f)
            {
                __instance.healthposy = -2f;
            }
            __instance.healthfill.transform.localPosition = new Vector3(x + num4, __instance.healthposy, 0f);
            if (__instance.currenthealth <= 0.01f)
            {
                __instance.healthzerotimer += Time.deltaTime;
            }
            else if (__instance.healthzerotimer > 0f && __instance.currenthealth > 0.01f)
            {
                __instance.healthzerotimer = 0f;
            }
            if(__instance.leveleditor) editorFunc(__instance);
            if ((!__instance.leveleditor && !__instance.freeplay) || __instance.playingineditor)
            {
                float num8 = __instance.musictrack.time - __instance.latency_offset - __instance.noteoffset;
                if (__instance.musictrack.time > __instance.levelendtime && __instance.levelendtime > 0f && !__instance.level_finshed && !__instance.quitting)
                {
                    __instance.level_finshed = true;
                    Debug.Log("====== LEVEL DONE! ======");
                    __instance.curtainc.closeCurtain(true);
                    GlobalVariables.gameplay_scoretotal = __instance.totalscore;
                    GlobalVariables.gameplay_scoreperc = (float)__instance.totalscore / (float)__instance.maxlevelscore;
                    Debug.Log(string.Concat(new object[]
                    {
                        "score percentage: ",
                        GlobalVariables.gameplay_scoreperc,
                        "(",
                        __instance.totalscore,
                        "/",
                        __instance.maxlevelscore,
                        ")"
                    }));
                    GlobalVariables.gameplay_notescores = new int[]
                    {
                        __instance.scores_F,
                        __instance.scores_D,
                        __instance.scores_C,
                        __instance.scores_B,
                        __instance.scores_A
                    };
                }
                if (__instance.musictrack.time > __instance.tempotimer)
                {
                    __instance.tempotimer = 60f / __instance.tempo * (float)(__instance.beatnum + 1);
                    __instance.beatnum++;
                    __instance.timesigcount++;
                    if (__instance.timesigcount > __instance.beatspermeasure)
                    {
                        __instance.timesigcount = 1;
                        __instance.bgcontroller.tickbg();
                        if (__instance.beatspermeasure == 3)
                        {
                            __instance.flashLeftBounds();
                        }
                    }
                    if (__instance.beatspermeasure != 3)
                    {
                        __instance.flashLeftBounds();
                    }
                    if (__instance.breathscale == 1f)
                    {
                        __instance.breathscale = 0.75f;
                    }
                    else
                    {
                        __instance.breathscale = 1f;
                    }
                }
                if (__instance.musictrack.time > __instance.tempotimerdot)
                {
                    __instance.beatnumdot++;
                    __instance.tempotimerdot = 60f / (__instance.tempo * 4f) * (float)__instance.beatnumdot;
                    __instance.animatePlayerDot();
                }
                Vector3 anchoredPosition3D = __instance.noteholderr.anchoredPosition3D;
                __instance.noteholderr.anchoredPosition3D = new Vector3(__instance.zeroxpos + num8 * -__instance.trackmovemult, 0f, 0f);
                __instance.lyricsholderr.anchoredPosition3D = new Vector3(__instance.zeroxpos + num8 * -__instance.trackmovemult, 0f, 0f);
                if (!__instance.leveleditor)
                {
                    if (__instance.noteholderr.anchoredPosition3D.x - __instance.zeroxpos + __instance.allbeatlines[__instance.beatlineindex].anchoredPosition3D.x < 0f)
                    {
                        __instance.maxbeatlinex += (float)(__instance.defaultnotelength * __instance.beatspermeasure);
                        __instance.allbeatlines[__instance.beatlineindex].anchoredPosition3D = new Vector3(__instance.maxbeatlinex, 0f, 0f);
                        __instance.beatlineindex++;
                        if (__instance.beatlineindex > __instance.numbeatlines - 1)
                        {
                            __instance.beatlineindex = 0;
                        }
                    }
                    if (__instance.bgindex < __instance.bgdata.Count && num8 > __instance.bgdata[__instance.bgindex][0])
                    {
                        Debug.Log("WAH!");
                        __instance.bgcontroller.bgMove((int)__instance.bgdata[__instance.bgindex][1]);
                        __instance.bgindex++;
                    }
                }
            }
            
            bool flag = false;
            int num9 = 0;
            for (int k = 0; k < __instance.toot_keys.Count; k++)
            {
                if (Input.GetKey(__instance.toot_keys[k]))
                {
                    num9++;
                }
                if(Input.GetKeyDown(__instance.toot_keys[k])){
                    flag = true;
                }
            }
            if (Input.GetMouseButton(0) && !__instance.quitting && !__instance.enteringlyrics)
            {
                num9++;
                if(Input.GetMouseButtonDown(0)) flag = true;
            }
            if (!__instance.freeplay)
            {
                __instance.scorecounter += Time.deltaTime;
                if (__instance.scorecounter > 0.01f)
                {
                    __instance.scorecounter = 0f;
                    __instance.tallyScore();
                }
            }
            if (!__instance.leveleditor)
            {
                float num10 = __instance.noteholderr.anchoredPosition3D.x - __instance.zeroxpos;
                if (num10 > 0f)
                {
                    num10 = -1f;
                }
                else
                {
                    num10 = Mathf.Abs(num10);
                }
                
                if (num10 > __instance.currentnotestart && !__instance.noteactive)
                {
                    __instance.noteactive = true;
                    Debug.Log("note " + __instance.currentnoteindex + " START");
                    if (__instance.currentnoteindex > 0)
                    {
                        float[] array2 = __instance.allnotevals[__instance.currentnoteindex - 1];
                        if (Mathf.Abs(__instance.currentnotestart - array2[1]) >= 0.01f && !__instance.released_button_between_notes)
                        {
                            Debug.Log("PLAYER DIDN'T RELEASE BUTTON BETWEEN NOTES!!");
                            __instance.multiplier = 0;
                            __instance.highestcombocounter = 0;
                            __instance.affectHealthBar(-15f);
                        }
                    }
                }
                else if (num10 > __instance.currentnoteend && __instance.noteactive)
                {
                    Debug.Log("note " + __instance.currentnoteindex + " END");
                    __instance.activateNextNote(__instance.currentnoteindex);
                    __instance.note_end_timer = __instance.max_note_end_timer;
                    __instance.noteactive = false;
                    __instance.released_button_between_notes = false;
                    if (!__instance.freeplay)
                    {
                        __instance.getScoreAverage();
                        __instance.grabNoteRefs(1);
                    }
                }
                if (__instance.noteactive && !__instance.freeplay)
                {
                    float num11 = (__instance.currentnoteend - num10) / (__instance.currentnoteend - __instance.currentnotestart);
                    num11 = Mathf.Abs(1f - num11);
                    float num12 = __instance.easeInOutVal(num11, 0f, __instance.currentnotepshift, 1f);
                    float f = __instance.pointerrect.anchoredPosition.y - (__instance.currentnotestarty + num12);
                    float num13 = 100f - Mathf.Abs(f);
                    if (!__instance.noteplaying)
                    {
                        num13 = 0f;
                    }
                    if (__instance.notescoreaverage == -1f)
                    {
                        __instance.notescoreaverage = num13;
                        if (__instance.notescoreaverage < 0f)
                        {
                            __instance.notescoreaverage = 0f;
                        }
                        Debug.Log("FIRST SAMPLE SCOREAVG: " + __instance.notescoreaverage);
                    }
                    else if (__instance.shortnote && __instance.shortnotecounter < 2 && num13 == 0f)
                    {
                        __instance.shortnotecounter++;
                    }
                    else
                    {
                        __instance.notescoresamples += 1f;
                        float num14 = 1f - 1f / __instance.notescoresamples;
                        float num15 = 1f / __instance.notescoresamples;
                        __instance.notescoreaverage = __instance.notescoreaverage * num14 + num13 * num15;
                        __instance.shortnotecounter = 0;
                    }
                }
                else if (!__instance.noteactive && !__instance.released_button_between_notes && (flag || num9 == 0))
                {
                    __instance.released_button_between_notes = true;
                }
            }
            if ((!__instance.leveleditor || __instance.playingineditor) && !__instance.controllermode && !__instance.autoplay)
            {
                float num16 = Input.mousePosition.y / (float)Screen.height;
                if (num16 < 0f)
                {
                    num16 = 0f;
                }
                else if (num16 > 1f)
                {
                    num16 = 1f;
                }
                num16 -= 0.5f;
                num16 *= __instance.mousemult * GlobalVariables.localsettings.sensitivity;
                if (GlobalVariables.mousecontrolmode == 1)
                {
                    num16 *= -1f;
                }
                if (!__instance.paused)
                {
                    __instance.puppet_humanc.doPuppetControl(num16 * 2f);
                    __instance.puppet_humanc.vibrato = __instance.vibratoamt;
                }
                Vector3 vector = new Vector3(__instance.zeroxpos - (float)__instance.dotsize * 0.5f, num16 * __instance.vsensitivity, 0f);
                if (vector.y > __instance.vbounds + __instance.outerbuffer)
                {
                    vector.y = __instance.vbounds + __instance.outerbuffer;
                }
                else if (vector.y < -__instance.vbounds - __instance.outerbuffer)
                {
                    vector.y = -__instance.vbounds - __instance.outerbuffer;
                }
                Vector3 localPosition = __instance.pointer.transform.localPosition;
                Vector3 b = (vector - localPosition) * 0.42f;
                __instance.pointer.transform.localPosition = localPosition + b;
            }
            if (Input.GetKey(KeyCode.Escape) && !__instance.quitting && __instance.musictrack.time > 0.5f && !__instance.freeplay)
            {
                __instance.musictrack.Pause();
                __instance.sfxrefs.backfromfreeplay.Play();
                __instance.curtainc.closeCurtain(false);
                __instance.paused = true;
                __instance.quitting = true;
                __instance.pausecanvas.SetActive(true);
                __instance.pausecontroller.showPausePanel();
            }
            else if (Input.GetKey(KeyCode.Escape) && !__instance.quitting && __instance.freeplay && __instance.curtainc.doneanimating)
            {
                __instance.sfxrefs.backfromfreeplay.Play();
                __instance.curtainc.closeCurtain(true);
                __instance.paused = true;
                __instance.quitting = true;
            }
            if (!__instance.controllermode)
            {
                //float num17 = Input.mousePosition.y / (float)Screen.height;
                if (flag && !__instance.outofbreath && __instance.readytoplay) //&& num17 < 0.95f
                {
                    __instance.setPuppetShake(true);
                    __instance.playNote();
                    __instance.noteplaying = true;
                }
                else if (num9 == 0 && __instance.noteplaying && !__instance.autoplay)
                {
                    __instance.setPuppetShake(false);
                    __instance.stopNote();
                    __instance.noteplaying = false;
                    
                }
            }
            if (__instance.noteplaying)
            {
                if (__instance.currentnotesound.time > __instance.currentnotesound.clip.length - 1.25f)
                {
                    __instance.currentnotesound.time = 1f;
                }
                float num18 = Mathf.Pow(__instance.notestartpos - __instance.pointer.transform.localPosition.y, 2f) * 6.8E-06f;
                float num19 = (__instance.notestartpos - __instance.pointer.transform.localPosition.y) * (1f + num18);
                if (num19 > 0f)
                {
                    num19 = (__instance.notestartpos - __instance.pointer.transform.localPosition.y) * 1.392f;
                    num19 *= 0.5f;
                }
                __instance.currentnotesound.pitch = 1f - num19 * __instance.pitchamount;
                if (__instance.currentnotesound.pitch > 2f)
                {
                    __instance.currentnotesound.pitch = 2f;
                }
                else if (__instance.currentnotesound.pitch < 0.5f)
                {
                    __instance.currentnotesound.pitch = 0.5f;
                }
                if (__instance.breathcounter < 1f)
                {
                    __instance.breathcounter += Time.deltaTime * 0.22f;
                    if (__instance.breathcounter > 1f)
                    {
                        __instance.breathcounter = 1f;
                        Debug.Log("OUT OF BREATH");
                        __instance.sfxrefs.outofbreath.Play();
                        __instance.breathglow.anchoredPosition3D = new Vector3(-380f, 0f, 0f);
                        __instance.outofbreath = true;
                        __instance.noteplaying = false;
                        __instance.setPuppetShake(false);
                        __instance.setPuppetBreath(true);
                        __instance.stopNote();
                    }
                }
            }
            else if (!__instance.noteplaying && __instance.breathcounter > 0f)
            {
                if (!__instance.outofbreath)
                {
                    __instance.breathcounter -= Time.deltaTime * 8.5f;
                }
                else if (__instance.outofbreath)
                {
                    __instance.breathcounter -= Time.deltaTime * 0.29f;
                    __instance.breathglow.anchoredPosition3D = new Vector3(-380f + (__instance.breathcounter - 1f) * 100f, 0f, 0f);
                }
                if (__instance.breathcounter < 0f)
                {
                    __instance.breathcounter = 0f;
                    if (__instance.outofbreath)
                    {
                        __instance.outofbreath = false;
                        __instance.setPuppetBreath(false);
                        Debug.Log("breath back");
                    }
                }
            }
            float x2 = 37f - 72f * __instance.breathcounter;
            float num20 = __instance.topbreathr.anchoredPosition3D.y;
            num20 += (__instance.breathcounter + 0.5f) * 3f;
            if (num20 > -85f)
            {
                num20 = -141f;
            }
            float num21 = __instance.bottombreathr.anchoredPosition3D.y;
            num21 -= (__instance.breathcounter + 0.5f) * 3f;
            if (num21 < -42f)
            {
                num21 = 14f;
            }
            __instance.topbreathr.anchoredPosition3D = new Vector3(x2, num20, 0f);
            __instance.bottombreathr.anchoredPosition3D = new Vector3(x2, num21, 0f);
            return false;
        }

        private static void editorFunc(GameController __instance){
            if (__instance.leveleditor)
            {
                if (Input.GetKeyDown(KeyCode.Return) && !__instance.enteringlyrics)
                {
                    __instance.playFromPlayhead();
                }
                if (!__instance.playingineditor && !__instance.levelnamefield.isFocused && !__instance.enteringlyrics)
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            __instance.changeEditorTime(1);
                        }
                        else if (Input.GetKey(KeyCode.LeftShift))
                        {
                            __instance.changeEditorTempo(-1);
                        }
                        else if (Input.GetKey(KeyCode.LeftAlt))
                        {
                            __instance.changeTimeSig(-1);
                        }
                        else
                        {
                            __instance.moveCursor(1);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            __instance.changeEditorTime(-1);
                        }
                        else if (Input.GetKey(KeyCode.LeftShift))
                        {
                            __instance.changeEditorTempo(1);
                        }
                        else if (Input.GetKey(KeyCode.LeftAlt))
                        {
                            __instance.changeTimeSig(1);
                        }
                        else
                        {
                            __instance.moveCursor(-1);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            __instance.changeLineSpacing(true, -1);
                        }
                        else
                        {
                            __instance.moveTimeline(-1);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            __instance.changeLineSpacing(true, 1);
                        }
                        else
                        {
                            __instance.moveTimeline(1);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.N))
                    {
                        __instance.enterNote();
                    }
                    else if (Input.GetKeyDown(KeyCode.M))
                    {
                        float num5 = Mathf.Floor((__instance.noteholderr.anchoredPosition3D.x - __instance.zeroxpos) / (float)(-(float)__instance.defaultnotelength));
                        __instance.editorposition = num5;
                        LeanTween.moveLocal(__instance.noteholder, new Vector3(__instance.zeroxpos - num5 * (float)__instance.defaultnotelength, 0f, 0f), 0.15f).setEaseInOutQuad();
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        __instance.tryToDeleteNote();
                    }
                    else if (Input.GetKeyDown(KeyCode.Z))
                    {
                        __instance.tryToDeleteLyric(true);
                    }
                    else if (Input.GetKeyDown(KeyCode.O))
                    {
                        __instance.resetNoteColor();
                    }
                    else if (Input.GetKeyDown(KeyCode.B))
                    {
                        if (!__instance.enteringlyrics)
                        {
                            __instance.clearAllBGData();
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.H))
                    {
                        __instance.editorcopySTART = __instance.editorposition;
                    }
                    else if (Input.GetKeyDown(KeyCode.J))
                    {
                        __instance.editorcopyEND = __instance.editorposition;
                    }
                    else if (Input.GetKeyDown(KeyCode.K))
                    {
                        __instance.editorcopyPASTE = __instance.editorposition;
                        float num6 = __instance.editorcopyPASTE - __instance.editorcopySTART;
                        List<float[]> list = new List<float[]>();
                        for (int i = 0; i < __instance.leveldata.Count; i++)
                        {
                            float num7 = __instance.leveldata[i][0];
                            if (num7 >= __instance.editorcopySTART && num7 <= __instance.editorcopyEND)
                            {
                                float[] item = new float[]
                                {
                                    __instance.leveldata[i][0],
                                    __instance.leveldata[i][1],
                                    __instance.leveldata[i][2],
                                    __instance.leveldata[i][3],
                                    __instance.leveldata[i][4]
                                };
                                list.Add(item);
                            }
                        }
                        Debug.Log(list.Count + " notes.");
                        for (int j = 0; j < list.Count; j++)
                        {
                            float[] array = list[j];
                            array[0] += num6;
                            __instance.pasteNote(array);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.L))
                    {
                        Debug.Log("ENTERING LYRIC!!");
                        __instance.enteringlyrics = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.E))
                    {
                        __instance.enterEndpoint();
                    }
                    else if (Input.GetKeyDown(KeyCode.I))
                    {
                    }
                }
                else if (!__instance.playingineditor && !__instance.levelnamefield.isFocused && __instance.enteringlyrics)
                {
                    foreach (object obj in Enum.GetValues(typeof(KeyCode)))
                    {
                        KeyCode key = (KeyCode)obj;
                        if (Input.GetKeyDown(key))
                        {
                            string text = key.ToString();
                            if (text == "Backspace")
                            {
                                if (__instance.editorlyrictxt.Length != 0)
                                {
                                    __instance.editorlyrictxt = __instance.editorlyrictxt.Substring(0, __instance.editorlyrictxt.Length - 1);
                                }
                            }
                            else if (text == "Return")
                            {
                                if (__instance.editorlyrictxt == "")
                                {
                                    Debug.Log("Press L and type a lyric first");
                                }
                                else
                                {
                                    float xpos = __instance.editorposition;
                                    float y = __instance.pointerrect.transform.localPosition.y;
                                    __instance.enterLyric(xpos, y, __instance.editorlyrictxt, true);
                                    __instance.editorlyrictxt = "";
                                    __instance.enteringlyrics = false;
                                }
                            }
                            else if (text == "Space" || text == "space")
                            {
                                __instance.editorlyrictxt += " ";
                                Debug.Log(__instance.editorlyrictxt);
                            }
                            else if (text == "Quote" || text == "quote")
                            {
                                __instance.editorlyrictxt += "'";
                                Debug.Log(__instance.editorlyrictxt);
                            }
                            else if (text.Length == 1)
                            {
                                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                                {
                                    __instance.editorlyrictxt += text.ToLower();
                                }
                                else
                                {
                                    __instance.editorlyrictxt += text;
                                }
                                Debug.Log(__instance.editorlyrictxt);
                            }
                            else if (text == "minus" || text == "Minus")
                            {
                                __instance.editorlyrictxt += "-";
                            }
                        }
                    }
                }
            }
        }
    }
}