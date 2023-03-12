<CsoundSynthesizer>
<CsOptions>
-odac
-+rtmidi=NULL -M0
</CsOptions>
<CsInstruments>
; Initialize the global variables. 
ksmps = 32
nchnls = 2
0dbfs = 1
sr = 44100
ga1 init 0 
// create global signal arrays for delay and reverb, max of 10 instances because of this static array size
gaDelay[]     init       10
gaReverb[]     init       10

;instrument will be triggered by keyboard widget
instr 1
    klfo line 0, 0.1, 1
    al   lfo klfo, 5, 0
    iwetamt = p6
    idryamt = 1 - p6 
    ; //aOut poscil p5, cpspch(p4)+al, 1

    ;;;; START SETUP VIBRATO ;;;;;
    ; kaverageamp     init .5
    ; kaveragefreq    init 5
    ; krandamountamp  line 15, .01, 20			;increase random amplitude of vibrato
    ; krandamountfreq init .3
    ; kampminrate init 3
    ; kampmaxrate init 5
    ; kcpsminrate init 3
    ; kcpsmaxrate init 5
    ; kvib vibrato kaverageamp, kaveragefreq, krandamountamp, krandamountfreq, kampminrate, kampmaxrate, kcpsminrate, kcpsmaxrate, 1
    ;;;; END SETUP VIBRATO ;;;;;

      ipitchStart = cpspch(p25)
      ipitchEnd = cpspch(p26)
      ifreq    = cpspch(p4)
      iamp     = p3
      ilowamp  = 0              ; determines amount of lowpass output in signal
      ihighamp = 0             ; determines amount of highpass output in signal
      ibandamp = 0             ; determines amount of bandpass output in signal

      ; check which type of frequency filter requested
      if (p31 == 0) then 
        ilowamp = 1
      elseif (p31 == 1) then 
        ihighamp = 1
      elseif (p31 == 2) then
        ibandamp = 1
      endif

      iq       = p22          ; value of q
      icutoff = p21
      ifilteramount = p27
  
        ; MIN IS 3 OCTAVES LOWER THAN ifreq
      kEnv linsegr ipitchStart, p13, ifreq, p14, p15*ifreq, p16, ipitchEnd ;pitch envelope 
      ;kEnv madsr p13, p14, p15, p16 ;pitch envelope

      ; Oscillator for creating sound

      if (p11 == 6) then
        aOut  noise p5, 0
        aOut  clip aOut, 2, .9	;clip signal
      else 
        ; check if pitch modulation enabled 
        if (p29 == 1) then  
          aOut poscil p5, kEnv, p11
        else
          aOut poscil p5, ifreq, p11
        endif  
      endif
    
      
      aEnv linsegr 0, p7, 1, p8, p9, p10, 0
      ;aEnv madsr p7, p8, p9, p10 ;amplitude envelope
      
      a1 clfilt aOut*aEnv, p12, 0, 10 ; global lowpass
      
    
      ; check if frequency filter modulation enabled 
      if (p30 == 1) then
        a2Env linsegr icutoff, p17, icutoff + (8000-icutoff)*(ifilteramount^2), p18, icutoff + (8000-icutoff)*(ifilteramount^2) * (p19^2), p20, icutoff ;;release time always same as amp adsr
        
        ; create low high and band from filter
        alow, ahigh, aband   svfilter a1, a2Env, iq
          ; multiply with param and sum together
        aout1   =         alow * ilowamp
        aout2   =         ahigh * ihighamp
        aout3   =         aband * ibandamp
        asum    =         aout1 + aout2 + aout3
      else 
        asum = a1
      endif
    
    outs asum * p23, asum * p24


    ga1 += asum * iwetamt

    if (p28 > -1) then
      gaDelay[p28] = gaDelay[p28] + asum
    endif

    if (p32 > -1) then
      gaReverb[p32] = gaReverb[p32] + (asum * iwetamt)
    endif
    
    
endin


instr 98 ; delay
    
    adel init 0
    adel	delay  gaDelay[p4] + (adel * p5), p6
    outs adel, adel
    gaDelay[p4] = 0
endin

instr 99	;(highest instr number executed last)

      a1 clfilt ga1, 1000, 1, 10
; create reverberated version of input signal (note stereo input and output)
      aRvbL,aRvbR  freeverb  a1, a1,p5,p6, sr, 0
      outs      aRvbL, aRvbR ; send audio to outputs
      ga1  = 0	;clear
endin

</CsInstruments>
<CsScore>
;causes Csound to run for about 7000 years...
f0 z

; create table for different waveforms
f1 0 16384 10 1                                          ; Sine
f2 0 16384 10 1 0.5 0.3 0.25 0.2 0.167 0.14 0.125 .111   ; Sawtooth
f3 0 16384 10 1 0   0.3 0    0.2 0     0.14 0     .111   ; Square
f4 0 256 7 -1 128 1 128 -1                               ; Triangle
f5 0 16384 10 1 1   1   1    0.7 0.5   0.3  0.1          ; Pulse
; id 6 is officially noise but is not made as table



i99.0 0 -1 0 0.85 0.5 
</CsScore>

</CsoundSynthesizer>