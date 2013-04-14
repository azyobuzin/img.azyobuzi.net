#!/virtual/azyobuzin/local/bin/python
# -*- coding: utf-8 -*-

import cgitb
cgitb.enable() #try ブロック外でエラー起こったらつらい

import cgi
import datetime
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

def print_result():
    print "Status: " + str(status)
    for header in headers.iteritems():
        print header[0] + ": " + header[1]
    print
    print body,

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
    print_result()
    exit()

def iter_resolvers():
    "呼び出すたびに Resolver を列挙します。これは通常の CGI での利用を想定したチューニングです。"
    resolvers_members = dir(resolvers)
    for module_name in (filename[0:-3] for filename in sorted(os.listdir("resolvers")) if filename.endswith(".py") and filename != "__init__.py"):
        m = getattr(__import__("resolvers." + module_name), module_name)
        classes = (
            c for c in
                (getattr(m, class_name) for class_name in dir(m) if not (class_name.startswith("_") or class_name in resolvers_members))
            if isinstance(c, type) and issubclass(c, resolvers.Resolver))
        for res in classes:
            yield res()

def search_resolver(uri):
    for resolver in iter_resolvers():
        match = resolver.regex.match(uri)
        if match is not None:
            return (resolver, match)
    return (None, None)

def get_imageuri(resolver, match, size, use_https):
    if size == "full":
        return resolver.get_full_https(match) if use_https else resolver.get_full(match)
    elif size == "large":
        return resolver.get_large_https(match) if use_https else resolver.get_large(match)
    elif size == "thumb":
        return resolver.get_thumb_https(match) if use_https else resolver.get_thumb(match)
    elif size == "video":
        return resolver.get_video_https(match) if use_https else resolver.get_video(match)
    raise ValueError()

def set_expires(days):
    expires = datetime.datetime.utcnow() + datetime.timedelta(days=days)
    headers["expires"] = expires.strftime("%a, %d %b %Y %H:%M:%S GMT")

try:
    if os.environ['REQUEST_METHOD'] not in ("GET", "HEAD"):
        set_error(4051, None)

    form = cgi.FieldStorage()

    if "api" in form:
        api = form["api"].value

        if api == "regex.json":
            body = json.dumps([{"name": resolver.service_name, "regex": resolver.regex_str} for resolver in iter_resolvers()])
        elif api in ("redirect", "redirect.json"):
            if "uri" not in form:
                set_error(4001, None)
            uri = form["uri"].value
            size = form.getvalue("size", "full")
            if size not in ("full", "large", "thumb", "video"):
                set_error(4003, None)
            use_https = form.getvalue("use_https", "false").lower() == "true"

            resolver, match = search_resolver(uri)
            if resolver is None:
                set_error(4002, None)

            result = get_imageuri(resolver, match, size, use_https)
            if not result and use_https:
                result = get_imageuri(resolver, match, size, False)

            if not result:
                if size == "video":
                    set_error(4045, None)
                else:
                    set_error(4043, None)

            status = 302
            headers["Location"] = result
            set_expires(10)
        elif api == "all_sizes.json":
            if "uri" not in form:
                set_error(4001, None)
            uri = form["uri"].value

            resolver, match = search_resolver(uri)
            if resolver is None:
                set_error(4002, None)

            body = json.dumps({
                "service": resolver.service_name,
                "full": resolver.get_full(match),
                "full_https": resolver.get_full_https(match),
                "large": resolver.get_large(match),
                "large_https": resolver.get_large_https(match),
                "thumb": resolver.get_thumb(match),
                "thumb_https": resolver.get_thumb_https(match),
                "video": resolver.get_video(match),
                "video_https": resolver.get_video_https(match)
            })

            set_expires(10)
        else:
            set_error(4042, None)
    else:
        set_error(4041, None)
except resolvers.PictureNotFoundError as e:
    set_error(4043, e)
except resolvers.IsNotPictureError as e:
    set_error(4044, e)
except Exception as e:
    set_error(5000, e)

print_result()
