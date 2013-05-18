# -*- coding: utf-8 -*-

appdir = "/var/www/test/img3"

import sys
sys.path.append(appdir)

import datetime
import json
import os
import traceback

from werkzeug.exceptions import HTTPException, NotFound
from werkzeug.routing import Map, Rule
from werkzeug.wrappers import Request, Response

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

url_map = Map([
    Rule("/", endpoint=index),
    Rule("/regex.json", endpoint=regex, methods=["GET"])
])

@Request.application
def application(request):
    try:
       endpoint, values = url_map.bind_to_environ(request.environ).match()
       return endpoint(request)
    except NotFound as e:
        return error_response(4042, e)
    except HTTPException as e:
        return e
