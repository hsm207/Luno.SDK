import json

with open('docs/luno_api_spec.json', 'r') as f:
    spec = json.load(f)

public_endpoints = []
private_endpoints = []

for path, path_item in spec.get('paths', {}).items():
    for method, operation in path_item.items():
        if method not in ['get', 'post', 'put', 'delete', 'patch']:
            continue
        security = operation.get('security', [])
        if len(security) == 0:
            public_endpoints.append(f"{method.upper()} {path}")
        else:
            private_endpoints.append(f"{method.upper()} {path}")

print("PUBLIC ENDPOINTS:")
for p in public_endpoints:
    print(p)
