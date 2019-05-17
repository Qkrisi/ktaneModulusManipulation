using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

/// <summary>
/// Custom module made by hockeygoalie78
/// Changes the rules using the number of other modules remaining and a pre-defined ruleset.
/// </summary>
public class ModulusManipulation : MonoBehaviour
{
    public KMBombInfo bombInfo;
    public KMAudio bombAudio;
    public KMSelectable upButtonLeft;
    public KMSelectable upButtonMiddle;
    public KMSelectable upButtonRight;
    public KMSelectable downButtonLeft;
    public KMSelectable downButtonMiddle;
    public KMSelectable downButtonRight;
    public KMSelectable submitButton;
    public TextMesh leftText;
    public TextMesh middleText;
    public TextMesh rightText;
    private KMBombModule bombModule;

    private int startingNumber;
    private int finalSolution;
    private int submission;
    private int[] digits;

    private string serialNumber;
    private char[] serialNumberArray;
    private int batteryCount;
    private int aaBatteryCount;
    private int dBatteryCount;
    private bool serialNumberSpecial; //Serial number contains 3 or 6
    private bool serialLetterSpecial; //Serial number least 4 letters
    private bool serialVowel;
    private bool serialLastDigitEven;
    private int litIndicatorCount;
    private int unlitIndicatorCount;
    private bool containsSpecificPorts; //Contains a PS/2, RJ-45, or Serial port
    private int otherModsRemainingCount;
    private int strikeCount;
    private bool minutesRemainingIsEven;

    private static int moduleIdCounter = 1;
    private int moduleId;

    void Start ()
    {
        //Set module ID
        moduleId = moduleIdCounter++;

        //Set up button interactions
        upButtonLeft.OnInteract += delegate { ChangeNumber(0); return false; };
        upButtonMiddle.OnInteract += delegate { ChangeNumber(1); return false; };
        upButtonRight.OnInteract += delegate { ChangeNumber(2); return false; };
        downButtonLeft.OnInteract += delegate { ChangeNumber(3); return false; };
        downButtonMiddle.OnInteract += delegate { ChangeNumber(4); return false; };
        downButtonRight.OnInteract += delegate { ChangeNumber(5); return false; };
        submitButton.OnInteract += delegate { CheckSolution(); return false; };

        //Set starting numbers randomly
        digits = new int[3];
        digits[0] = Random.Range(0, 10);
        leftText.text = digits[0].ToString();
        digits[1] = Random.Range(0, 10);
        middleText.text = digits[1].ToString();
        digits[2] = Random.Range(0, 10);
        rightText.text = digits[2].ToString();
        Debug.LogFormat(@"[Modulus Manipulation #{0}] Starting number is {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);

        //Calculate the starting number using the first 3 characters in the serial number
        serialNumber = bombInfo.GetSerialNumber();
        serialNumberArray = serialNumber.ToCharArray();
        startingNumber = 0;
        if(char.IsDigit(serialNumberArray[0]))
        {
            startingNumber += (int)char.GetNumericValue(serialNumberArray[0]) * 100;
        }
        else
        {
            startingNumber += (serialNumberArray[0] % 32 % 10) * 100;
        }
        if(char.IsDigit(serialNumberArray[1]))
        {
            startingNumber += (int)char.GetNumericValue(serialNumberArray[1]) * 10;
        }
        else
        {
            startingNumber += (serialNumberArray[1] % 32 % 10) * 10;
        }
        if(char.IsDigit(serialNumberArray[2]))
        {
            startingNumber += (int)char.GetNumericValue(serialNumberArray[2]);
        }
        else
        {
            startingNumber += serialNumberArray[2] % 32 % 10;
        }

        //Battery counts
        aaBatteryCount = bombInfo.GetBatteryCount(2) + bombInfo.GetBatteryCount(3) + bombInfo.GetBatteryCount(4);
        dBatteryCount = bombInfo.GetBatteryCount(1);
        batteryCount = aaBatteryCount + dBatteryCount;

        //Serial number contains 3 or 6
        serialNumberSpecial = serialNumber.IndexOf("3") >= 0 || serialNumber.IndexOf("6") >= 0;

        //Serial number contains 4 letters
        serialLetterSpecial = bombInfo.GetSerialNumberLetters().Count() >= 4;

        //Serial number contains a vowel
        serialVowel = serialNumber.Any("AEIOU".Contains);

        //Last digit of serial number is even
        serialLastDigitEven = (int)char.GetNumericValue(serialNumberArray[5]) % 2 == 0;

        //Indicator counts
        litIndicatorCount = bombInfo.GetOnIndicators().Count();
        unlitIndicatorCount = bombInfo.GetOffIndicators().Count();

        //Contains a PS/2, RJ-45, or Serial port
        containsSpecificPorts = bombInfo.IsPortPresent("PS2") || bombInfo.IsPortPresent("RJ45") || bombInfo.IsPortPresent("Serial");

        //Other variables as needed
        bombModule = GetComponent<KMBombModule>();
    }

    /// <summary>
    /// Helper method that will change the numbers based on which arrow button is pressed
    /// </summary>
    /// <param name="index">Index of the button pushed (0 is top-left, then goes along the rows; 5 is bottom-right)</param>
    private void ChangeNumber(int index)
    {
        switch(index)
        {
            //Add to left digit
            case 0:
                digits[0] = (digits[0] + 1) % 10;
                leftText.text = digits[0].ToString();
                upButtonLeft.AddInteractionPunch(.2f);
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Display 1's up button was pressed. Number is now {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);
                break;
            //Add to middle digit
            case 1:
                digits[1] = (digits[1] + 1) % 10;
                middleText.text = digits[1].ToString();
                upButtonMiddle.AddInteractionPunch(.2f);
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Display 2's up button was pressed. Number is now {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);
                break;
            //Add to right digit
            case 2:
                digits[2] = (digits[2] + 1) % 10;
                rightText.text = digits[2].ToString();
                upButtonRight.AddInteractionPunch(.2f);
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Display 3's up button was pressed. Number is now {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);
                break;
            //Subtract from left digit
            case 3:
                digits[0] = (digits[0] + 9) % 10;
                leftText.text = digits[0].ToString();
                downButtonLeft.AddInteractionPunch(.2f);
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Display 1's down button was pressed. Number is now {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);
                break;
            //Subtract from middle digit
            case 4:
                digits[1] = (digits[1] + 9) % 10;
                middleText.text = digits[1].ToString();
                downButtonMiddle.AddInteractionPunch(.2f);
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Display 2's down button was pressed. Number is now {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);
                break;
            //Subtract from right digit
            case 5:
                digits[2] = (digits[2] + 9) % 10;
                rightText.text = digits[2].ToString();
                downButtonRight.AddInteractionPunch(.2f);
                bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Display 3's down button was pressed. Number is now {1}{2}{3}.", moduleId, digits[0], digits[1], digits[2]);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Helper method to determine whether or not the correct number has been entered and react appropriately
    /// </summary>
    private void CheckSolution()
    {
        //Movement/audio
        submitButton.AddInteractionPunch(.5f);
        bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);

        //Initial submission value
        submission = (digits[0] * 100) + (digits[1] * 10) + digits[2];
        Debug.LogFormat(@"[Modulus Manipulation #{0}] Submitted value {1}.", moduleId, submission);

        //Set up values calculated at solution check
        finalSolution = startingNumber;
        otherModsRemainingCount = bombInfo.GetSolvableModuleNames().Count() - bombInfo.GetSolvedModuleNames().Count() - 1;
        strikeCount = bombInfo.GetStrikes();
        minutesRemainingIsEven = ((int)bombInfo.GetTime() / 60) % 2 == 0;
        Debug.LogFormat(@"[Modulus Manipulation #{0}] Starting value for calculations is {1}.", moduleId, startingNumber);
        Debug.LogFormat(@"[Modulus Manipulation #{0}] There are currently {1} other modules remaining.", moduleId, otherModsRemainingCount);

        //Apply calculations
        if(otherModsRemainingCount % 5 == 0)
        {
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Rule set 5 is applicable.", moduleId);
            //If the bomb has more than one battery, add 400
            if(batteryCount > 1)
            {
                finalSolution += 400;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has more than one battery, adding 400. Solution is now {1}.", moduleId, finalSolution);
            }
            //If the serial number contains the number 3 or 6, subtract 40
            if(serialNumberSpecial)
            {
                finalSolution -= 40;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Serial number contains a 3 or 6, subtracting 40. Solution is now {1}.", moduleId, finalSolution);
            }
        }
        if(otherModsRemainingCount % 4 == 0)
        {
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Rule set 4 is applicable.", moduleId);
            //If the bomb has at least one AA battery and at least one D battery, multiply by 2
            if(aaBatteryCount >= 1 && dBatteryCount >= 1)
            {
                finalSolution *= 2;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has at least one AA battery and one D battery, multiplying by 2. Solution is now {1}.", moduleId, finalSolution);
            }
            //If the serial number has 4 letters, subtract 290
            if(serialLetterSpecial)
            {
                finalSolution -= 290;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Serial number has 4 letters, subtracting 290. Solution is now {1}.", moduleId, finalSolution);
            }
        }
        if(otherModsRemainingCount % 3 == 0)
        {
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Rule set 3 is applicable.", moduleId);
            //If the bomb has more than three batteries, subtract 160
            if(batteryCount > 3)
            {
                finalSolution -= 160;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has more than three batteries, subtracting 160. Solution is now {1}.", moduleId, finalSolution);
            }
            //If the bomb has more lit indicators than unlit indicators, add 75
            if(litIndicatorCount > unlitIndicatorCount)
            {
                finalSolution += 75;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has more lit indicators than unlit indicators, adding 75. Solution is now {1}.", moduleId, finalSolution);
            }
        }
        if(otherModsRemainingCount % 2 == 0)
        {
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Rule set 2 is applicable.", moduleId);
            //If the serial number has a vowel, add 340
            if(serialVowel)
            {
                finalSolution += 340;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Serial number has a vowel, adding 340. Solution is now {1}.", moduleId, finalSolution);
            }
            //If the bomb has a PS/2, RJ-45, or Serial port, add 180
            if(containsSpecificPorts)
            {
                finalSolution += 180;
                Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has a PS/2, RJ-45, or Serial port, adding 180. Solution is now {1}.", moduleId, finalSolution);
            }
        }
        //Anything modulus 1 is 0, so rule set 1 always applies and needs no if statement
        Debug.LogFormat(@"[Modulus Manipulation #{0}] Rule set 1 is applicable.", moduleId);
        //If the bomb has at least one strike, subtract 45
        if(strikeCount >= 1)
        {
            finalSolution -= 45;
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has at least one strike, subtracting 45. Solution is now {1}.", moduleId, finalSolution);
        }
        //If the bomb has any unlit indicators, subtract 15
        if(unlitIndicatorCount > 0)
        {
            finalSolution -= 15;
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Bomb has at least one unlit indicator, subtracting 15. Solution is now {1}.", moduleId, finalSolution);
        }
        //If the last digit of the serial number is even, add 150
        if(serialLastDigitEven)
        {
            finalSolution += 150;
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Last digit of the serial number is even, adding 150. Solution is now {1}.", moduleId, finalSolution);
        }
        //If the number of minutes remaining on the countdown timer is even (or 0), add 6
        if(minutesRemainingIsEven)
        {
            finalSolution += 6;
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Minutes remaining is even or 0, adding 6. Solution is now {1}.", moduleId, finalSolution);
        }

        //Compare final solution with input
        if(finalSolution < 0)
        {
            finalSolution = 0;
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Calculated solution was negative, so final solution is 0.", moduleId);
        }
        else
        {
            finalSolution %= 1000;
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Final solution is {1}.", moduleId, finalSolution);
        }
        if(submission == finalSolution)
        {
            bombModule.HandlePass();
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Submission was correct. Module passed.", moduleId);
        }
        else
        {
            bombModule.HandleStrike();
            Debug.LogFormat(@"[Modulus Manipulation #{0}] Submission was incorrect. Strike occurred.", moduleId);
        }
    }
}
