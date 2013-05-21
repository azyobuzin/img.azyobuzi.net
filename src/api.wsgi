# -*- coding: utf-8 -*-

appdir = "/var/www/test/img3" #環境にあわせて変更

import sys
sys.path.append(appdir)

import cgi
import datetime
import json
import os
import traceback

from werkzeug.exceptions import HTTPException, NotFound
from werkzeug.routing import Map, Rule
from werkzeug.wrappers import Request, Response

import resolvers
import resolvers.pixiv

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

mime_json = "application/json"

def iter_resolvers():
    resolvers_members = dir(resolvers)
    for module_name in (filename[0:-3] for filename in sorted(os.listdir(appdir + "/resolvers")) if filename.endswith(".py") and filename != "__init__.py"):
        m = getattr(__import__("resolvers." + module_name), module_name)
        classes = (
            c for c in
                (getattr(m, class_name) for class_name in dir(m) if not (class_name.startswith("_") or class_name in resolvers_members))
            if isinstance(c, type) and issubclass(c, resolvers.Resolver))
        for res in classes:
            yield res()
resolvers_list = list(iter_resolvers())

def search_resolver(uri):
    for resolver in resolvers_list:
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

def create_expires(days):
    expires = datetime.datetime.utcnow() + datetime.timedelta(days=days)
    return expires.strftime("%a, %d %b %Y %H:%M:%S GMT")

def error_response(error_code, e):
    error_tuple = errors[error_code]
    return Response(
        json.dumps({
            "error": {
                "code": error_code,
                "message":error_tuple[1],
                "exception": None if e is None else traceback.format_exc(e)
            }
        }),
        status=error_tuple[0],
        mimetype=mime_json)

def index(request):
    return error_response(4041, None)

def regex(request):
    return Response(
        json.dumps([{"name": resolver.service_name, "regex": resolver.regex_str} for resolver in resolvers_list]),
        status=200,
        mimetype=mime_json)

def redirect(request):
    uri = request.fs.getfirst("uri")
    size = request.fs.getfirst("size", "full")
    use_https = request.fs.getfirst("use_https", "false").lower() == "true"
    if not uri:
        return error_response(4001, None)
    if size not in ("full", "large", "thumb", "video"):
        return error_response(4003, None)
    
    resolver, match = search_resolver(uri)
    if not resolver:
        return error_response(4002, None)

    result = get_imageuri(resolver, match, size, use_https)
    if not result and use_https:
        result = get_imageuri(resolver, match, size, False)

    if not result:
        if size == "video":
            return error_response(4045, None)
        else:
            return error_response(4043, None)

    return Response("", status=302,
        headers={"Expires": create_expires(10), "Location": result})

def all_sizes(request):
    uri = request.fs.getfirst("uri")
    if not uri:
        return error_response(4001, None)

    resolver, match = search_resolver(uri)
    if not resolver:
        return error_response(4002, None)

    return Response(
        json.dumps({
            "service": resolver.service_name,
            "full": resolver.get_full(match),
            "full_https": resolver.get_full_https(match),
            "large": resolver.get_large(match),
            "large_https": resolver.get_large_https(match),
            "thumb": resolver.get_thumb(match),
            "thumb_https": resolver.get_thumb_https(match),
            "video": resolver.get_video(match),
            "video_https": resolver.get_video_https(match)
        }),
        status=200,
        headers={"Expires": create_expires(10)},
        mimetype=mime_json)

url_map = Map([
    Rule("/", endpoint=index),
    Rule("/regex.json", endpoint=regex, methods=["GET"]),
    Rule("/redirect", endpoint=redirect, methods=["GET"]),
    Rule("/redirect.json", endpoint=redirect, methods=["GET"]),
    Rule("/all_sizes.json", endpoint=all_sizes, methods=["GET"]),
    Rule("/pixiv_proxy.cgi", endpoint=resolvers.pixiv.pixiv_proxy, methods=["GET"])
])

@Request.application
def application(request):
    try:
       endpoint, values = url_map.bind_to_environ(request.environ).match()
       request.fs = cgi.FieldStorage(fp=request.environ['wsgi.input'], environ=request.environ)
       return endpoint(request)
    except NotFound as e:
        return error_response(4042, e)
    except HTTPException as e:
        return e
