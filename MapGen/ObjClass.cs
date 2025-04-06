﻿namespace MapGen;


[Flags]
public enum ObjClass {
	OC_NONE			=  0x00000000,
	OC_SPACESHIP	=  0x00000001,
	OC_PLANETOID	=  0x00000002,
	OC_MEXPLODE		=  0x00000004,
	OC_SHRAPNEL		=  0x00000008,
	OC_JUMPGATE		=  0x00000010,
	OC_NEBULA		=  0x00000020,
	OC_LAUNCHER		=  0x00000040,
	OC_WEAPON       =  0x00000080,
	OC_BLAST		=  0x00000100,
	OC_WAYPOINT		=  0x00000200,
	OC_PLATFORM		=  0x00000400,
	OC_FIGHTER		=  0x00000800,
	OC_LIGHT		=  0x00001000,
	OC_MINEFIELD    =  0x00002000,
	OC_TRAIL		=  0x00004000,
	OC_EFFECT		=  0x00008000,
	OC_FIELD		=  0x00010000,
	OC_NUGGET		=  0x00020000,
	OC_GROUP		=  0x00040000,
	OC_RESEARCH		=  0x00080000,
	OC_BLACKHOLE    =  0x00100000,
	OC_PLAYERBOMB	=  0x00200000,
	OC_BUILDRING	=  0x00400000,
	OC_BUILDOBJ		=  0x00800000,
	OC_MOVIECAMERA	=  0x01000000,
	OC_OBJECT_GENERATOR = 0x02000000,
	OC_TRIGGER		=  0x04000000,
	OC_SCRIPTOBJECT =  0x08000000,
	OC_UI_ANIM		=  0x10000000,
	CF_PLAYERALIGNED = 0x02200401,		// things to drop in the editor that have a player alignment

}

public enum FIELDCLASS
{
	FC_OTHER,
	FC_ASTEROIDFIELD=1,
	FC_MINEFIELD,
	FC_NEBULA,
	FC_ANTIMATTER
};

