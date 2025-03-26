namespace DACOM;

public enum GenResult {
	GR_OK						= 0x00000000,
	GR_GENERIC					= -1,
	GR_INVALID_PARMS			= -2,
	GR_INTERFACE_UNSUPPORTED	= -3,
	GR_OUT_OF_MEMORY			= -4,
	GR_OUT_OF_SPACE				= -5,
	GR_FILE_ERROR				= -6,
	GR_NOT_IMPLEMENTED			= -7,
	GR_DATA_NOT_FOUND			= -8
}
