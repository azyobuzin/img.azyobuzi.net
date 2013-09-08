from wsgiref.simple_server import make_server
import api

httpd = make_server("", 61482, api.application)
httpd.serve_forever()
