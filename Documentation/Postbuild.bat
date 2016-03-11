setlocal
cd %~dp0

echo Convert documents to TXT, XML and HTML formats

echo architecture
rfctool O:\Documents\Mesh\hallambaker-mesh-architecture.docx ^
	/html Publish\hallambaker-mesh-architecture.html ^
	/xml Publish\hallambaker-mesh-architecture.xml ^
	/txt Publish\hallambaker-mesh-architecture.txt

echo developer
rfctool O:\Documents\Mesh\hallambaker-mesh-developer.docx ^
	/html Publish\hallambaker-mesh-developer.html ^
	/xml Publish\hallambaker-mesh-developer.xml ^
	/txt Publish\hallambaker-mesh-developer.txt

echo reference
rfctool O:\Documents\Mesh\hallambaker-mesh-reference.docx ^
	/html Publish\hallambaker-mesh-reference.html ^
	/xml Publish\hallambaker-mesh-reference.xml ^
	/txt Publish\hallambaker-mesh-reference.txt

echo udf
rfctool O:\Documents\Mesh\hallambaker-udf.docx ^
	/html Publish\hallambaker-udf.html ^
	/xml Publish\hallambaker-udf.xml ^
	/txt Publish\hallambaker-udf.txt

echo reference
rfctool O:\Documents\Mesh\hallambaker-json-web-service.docx ^
	/html Publish\hallambaker-json-web-service.html ^
	/xml Publish\hallambaker-json-web-service.xml ^
	/txt Publish\hallambaker-json-web-service.txt

exit /b 0