#!/virtual/azyobuzin/local/bin/python
# -*- coding: utf-8 -*-

import cgi
import json
import os

import resolvers

errors = {
    4000: (400, "Bad request."),
    4001: (400, "\"uri\" parameter is required."),
    4002: (400, "\"uri\" parameter you requested is not supported."),
    4003: (400, "\"size\" parameter is invalid."),
    4040: (404, "Not Found."),
    4041: (404, "Select API."),
    4042: (404, "API you requested is not found."),
    4043: (404, "The picture you requested is not found."),
    4044: (404, "Your request is not a picture."),
    4045: (404, "Your request is not a video."),
    4050: (405, "The method is not allowed."),
    4051: (405, "Call with GET or HEAD method."),
    5000: (500, "Raised unknown exception on server.")
}

status = 200
headers = {"Content-Type": "application/json"}
body = ""

def set_error(error_code, ex):
    import traceback
    global status, body
    error_tuple = errors[error_code]
    status = error_tuple[0]
    body = json.dumps({
        "error": {
            "code": error_code,
            "message":error_tuple[1],
            "exception": None if ex is None else traceback.format_exc(ex)
        }
    })

def iter_resolvers():
    resolvers_members = dir(resolvers)
    for module_name in (filename[0:-3] for filename in os.listdir("resolvers") if filename.endswith(".py") and filename != "__init__.py"):
        m = getattr(__import__("resolvers." + module_name), module_name)
        classes = [
            c for c in
                (getattr(m, class_name) for class_name in dir(m) if not (class_name.startswith("_") or class_name in resolvers_members))
            if isinstance(c, type) and issubclass(c, resolvers.Resolver)]
        for res in classes:
            yield res()

try:
    if os.environ['REQUEST_METHOD'] not in ("GET", "HEAD"):
        set_error(4051, None)

    form = cgi.FieldStorage()

    if "api" in form:
        api = form["api"].value

        if api == "regex.json":
            body = json.dumps([{"name": resolver.service_name, "regex": resolver.regex_str} for resolver in iter_resolvers()])
        else:
            set_error(4042, None)
    else:
        set_error(4041, None)
except Exception, ex:
    set_error(5000, ex)

print "Status: " + str(status)
for header in headers.iteritems():
    print header[0] + ": " + header[1]
print
print body,
