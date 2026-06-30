from pathlib import Path
import re
xml_path = Path(r"C:\Users\Spencer Du\.nuget\packages\microsoft.openapi\2.7.5\lib\net8.0\Microsoft.OpenApi.xml")
dll_path = Path(r"C:\Users\Spencer Du\.nuget\packages\microsoft.openapi\2.7.5\lib\net8.0\Microsoft.OpenApi.dll")
package_path = xml_path.parent.parent
print('xml_path:', xml_path)
print('xml exists:', xml_path.exists())
print('dll_path:', dll_path)
print('dll exists:', dll_path.exists())
print('package_path exists:', package_path.exists())
print('package_path contents:', sorted([p.name for p in package_path.iterdir()]))
text = xml_path.read_text(encoding='utf-8')

def find_first(pattern):
    for line in text.splitlines():
        if pattern in line:
            return line
    return None

patterns = [
    'T:Microsoft.OpenApi.OpenApiSecurityScheme',
    'T:Microsoft.OpenApi.OpenApiReference',
    'T:Microsoft.OpenApi.OpenApiSecuritySchemeReference',
    'T:Microsoft.OpenApi.OpenApiReferenceableExtensions',
    'P:Microsoft.OpenApi.OpenApiSecurityScheme.Reference',
    'P:Microsoft.OpenApi.OpenApiSecurityScheme.',
    'T:Microsoft.OpenApi.OpenApiSecurityRequirement',
    'OpenApiSecuritySchemeReference',
    'ReferenceType',
    'Microsoft.OpenApi.Models',
]
for patt in patterns:
    print(f"{patt}: {patt in text}")
    if patt in text:
        print('  first:', find_first(patt))

print('\n--- Exact OpenApiReference type definitions ---')
for line in text.splitlines():
    if 'member name="T:Microsoft.OpenApi.OpenApiReference' in line:
        print(line)

print('\n--- OpenApiSecuritySchemeReference definitions ---')
for i, line in enumerate(text.splitlines()):
    if 'T:Microsoft.OpenApi.OpenApiSecuritySchemeReference' in line:
        start = max(0, i-5)
        end = min(len(text.splitlines()), i+40)
        for j in range(start, end):
            print(text.splitlines()[j])
        break

print('\n--- OpenApiSecurityScheme members ---')
for line in text.splitlines():
    if line.strip().startswith('<member name="P:Microsoft.OpenApi.OpenApiSecurityScheme.') or line.strip().startswith('<member name="M:Microsoft.OpenApi.OpenApiSecurityScheme.'):
        print(line.strip())

print('\n--- Lines containing OpenApiReference or ReferenceType ---')
for line in text.splitlines():
    if 'OpenApiReference' in line or 'ReferenceType' in line:
        print(line.strip())
        if 'member name="T:Microsoft.OpenApi.OpenApiReferenceableExtensions"' in line:
            break
