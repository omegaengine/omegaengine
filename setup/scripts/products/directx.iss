[CustomMessages]
directx_title=Microsoft DirectX June 2010 Minimal Runtime

en.directx_size=7 MB
de.directx_size=7 MB


[Code]
const
	directx_url = 'http://omegaengine.googlecode.com/files/directx-jun2010-minimal.exe';

procedure directx();
begin
	if not FileExists(GetEnv('windir') + '\system32\D3DX9_43.dll') then
		AddProduct('directx-jun2010-minimal.exe',
			'',
			CustomMessage('directx_title'),
			CustomMessage('directx_size'),
			directx_url);
end;
