# -*- coding: utf-8 -*-

from resolvers import *

class Pixiv(Resolver):
    @property
    def service_name(self):
        return "pixiv"

    @property
    def regex_str(self):
       return r"^http://(?:www\.)?pixiv\.net/(?:index|member_illust)\.php\?(?:.*)&?illust_id=(\d+)(?:&.*)?$"

    def get_full(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=full&id=" + match.group(1)

    def get_full_https(self, match):
        return None #誰か SSL サーバ証明書買って…

    def get_large(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=large&id=" + match.group(1)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return "http://img.azyobuzi.net/api/pixiv_proxy.cgi?size=thumb&id=" + match.group(1)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None

#pivix_proxy.cgi
from sgmllib import SGMLParser
import urllib
import urllib2

import MySQLdb
from werkzeug.wrappers import Response

from resolvers.private_constant import *

class PixivParser(SGMLParser):
    src_regex = re.compile("^(.+)_m(\\.\\w+)$")

    def __init__(self):
        SGMLParser.__init__(self)
        self.uri_prefix = None
        self.uri_extension = None
        self.flag = False

    def start_div(self, attributes):
        dic = dict(attributes)
        if "class" in dic and dic["class"] == "img-container":
            self.flag = True

    def do_img(self, attributes):
        if self.flag:
            match = PixivParser.src_regex.match(dict(attributes)["src"])
            self.uri_prefix = match.group(1)
            self.uri_extension = match.group(2)
            self.flag = False

def pixiv_proxy(request):
    def error_response(status, message):
        return Response(message, status=status, mimetype="text/plain")

    id = request.args.get("id")
    size = request.args.get("size", "full")
    if not id:
        return error_response(400, "\"id\" parameter is required.")
    if size not in ("full", "large", "thumb"):
        return error_response(400, "\"size\" parameter is invalid.")

    with MySQLdb.connect(host=db_host, port=db_port, user=db_user, passwd=db_password, db=db_name, charset="utf8") as c:
        c.execute("SELECT prefix, extension FROM pixiv WHERE id = %s", id)
        result = c.fetchone()

        if result:
            prefix = result[0]
            extension = result[1]
        else:
            response = urllib2.urlopen("http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + urllib.quote(id))
            html = response.read().decode("utf-8")

            if "<span class=\"error\">" in html:
                return error_response(404, "The picture is not found.")

            parser = PixivParser()
            parser.feed(html)
            parser.close()

            prefix = parser.uri_prefix
            extension = parser.uri_extension

            c.execute("INSERT INTO pixiv VALUES (%s, %s, %s)",
                (id, prefix, extension)
            )

    table = { "full": "", "large": "_m", "thumb": "_s" }

    req = urllib2.Request(prefix + table[size] + extension, headers={
        "User-Agent": "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.22 (KHTML, like Gecko) Chrome/25.0.1364.36 Safari/537.22",
        "Referer": "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + urllib.quote(id)
    })

    try:
        res = urllib2.urlopen(req)
    except urllib2.HTTPError as e:
        res = e

    return Response(res, status=res.code, content_type=res.headers["content-type"])
