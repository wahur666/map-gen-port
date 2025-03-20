namespace MapGen;

public class MapGen
{
    void GenerateMap (const struct FULLCQGAME & game, U32 seed, IPANIM * ipAnim) = 0;

    U32 GetBestSystemNumber (const FULLCQGAME & game,U32 approxNumber) = 0;

    U32 GetPosibleSystemNumbers (const FULLCQGAME & game, U32 * list) = 0;
}