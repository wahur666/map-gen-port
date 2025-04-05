namespace MapGen;

public class BASE_FIELD_DATA {
	public string terrainArchType;
	public ObjClass objClass;
	public FIELDCLASS fieldClass;

	public BASE_FIELD_DATA(string terrainArchType, ObjClass objClass, FIELDCLASS fieldClass) {
		this.terrainArchType = terrainArchType;
		this.objClass = objClass;
		this.fieldClass = fieldClass;
	}
}
