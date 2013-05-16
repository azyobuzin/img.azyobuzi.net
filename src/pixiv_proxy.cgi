#!/usr/bin/python
# -*- coding: utf-8 -*-

import cgitb
cgitb.enable()

import os
import re
from sgmllib import SGMLParser
import urllib2
import urlparse

import MySQLdb

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

query = urlparse.parse_qs(os.environ.get("QUERY_STRING", ""))

if "id" not in query:
    print "Status: 400"
    print "Content-Type: text/plain; charset=utf-8"
    print
    print "\"id\" parameter is required."
    exit()

id = query["id"][0]

size = query["size"][0] if "size" in query else "full"

if size not in ("full", "large", "thumb"):
    print "Status: 400"
    print "Content-Type: text/plain; charset=utf-8"
    print
    print "\"size\" parameter is invalid."
    exit()

db = MySQLdb.connect(user=db_user, passwd=db_password, db=db_name, charset="utf8")
c = db.cursor()
c.execute("SELECT prefix, extension FROM pixiv WHERE id = %s", id)
sqlResult = c.fetchone()

if sqlResult is None:
    httpRes = urllib2.urlopen("http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + id)
    html = httpRes.read().decode("utf-8")

    if html.find("<span class=\"error\">") != -1:
        print "Status: 404"
        print "Content-Type: text/plain; charset=utf-8"
        print
        print "The picture is not found."
        exit()

    parser = PixivParser()
    parser.feed(html)
    parser.close()

    prefix = parser.uri_prefix
    extension = parser.uri_extension

    c.execute("INSERT INTO pixiv VALUES (%s, %s, %s)",
        (id, prefix, extension)
    )

    db.commit()
else:
    prefix = sqlResult[0]
    extension = sqlResult[1]

table = { "full": "", "large": "_m", "thumb": "_s" }

req = urllib2.Request(prefix + table[size] + extension, headers={
    "User-Agent": "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.22 (KHTML, like Gecko) Chrome/25.0.1364.36 Safari/537.22",
    "Referer": "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=" + id
})

try:
    res = urllib2.urlopen(req)
except urllib2.HTTPError as ex:
    res = ex

print "Status: " + str(res.code)
print "Content-Type: " + res.headers["content-type"]
print

print res.read(),
