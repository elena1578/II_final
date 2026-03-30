using UnityEngine;

public enum EmotionType
{
    Neutral = 0,
    
    // tier 1
    Happy = 1,
    Sad = 2,
    Angry = 3,
    Afraid = 4,  // only used in context of hero fearing spiders

    // tier 2
    Ecstatic = 5, 
    Depressed = 6,
    Enraged = 7, 

    // tier 3
    Manic = 8, 
    Miserable = 9, 
    Furious = 10,

    // other
    Toast = 11,  // not actually an emotion but used to override emotion visuals
}
