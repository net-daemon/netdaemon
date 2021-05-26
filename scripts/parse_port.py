import sys
from urllib.parse import urlparse

parsed_url = urlparse(sys.argv[1])

print (parsed_url.port)
