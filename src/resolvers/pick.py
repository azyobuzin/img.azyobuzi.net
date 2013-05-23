## -*- coding: utf-8 -*-

#from HTMLParser import HTMLParser
#import urllib2

#from resolvers import *
#from resolvers.private_constant import *

#class PickParser(HTMLParser):
#    def __init__(self):
#        SGMLParser.__init__(self)
#        self.large_size = None
#        self.post_body_flag = False

#    def handle_starttag(self, tag, attrs):
#        dic = dict(attrs)
#        if tag == "div" and dic["class"] == "postBodyVisual largeSizeView":
#            self.post_body_flag = True

#        if tag == "img" and self.post_body_flag:
#            self.large_size = dic["src"]

#class Pick(StoringResolver):
#    @property
#    def service_name(self):
#        return "pick"

#    @property
#    def regex_str(self):
#        return r"^http://(?:pick\.naver\.jp/([\w\-%]+)/(\d+)|nav\.cx/(\w+))(?:\?.*)?$"

#    def get_parameters(self, match):
#        return {"username": match.group(1), "id": match.group(2), "shorten": match.group(3)}

#    def _work(self, param, cursor):
#        if param["shorten"]:
#            table = "pick_shorten"
#            columns = ["shorten", "username", "id"]
#            result = self.select_one(cursor, table, columns[1:], {columns[0]: param["shorten"]})

#            if result:
#                username = result[0]
#                id = result[1]
#            else:
#                response = urllib2.build_opener(DontRedirectHandler).open(
#                    Request2("http://nav.cx/" + param["shorten"], method="HEAD")
#                )

#                try:
#                    match = self.regex.match(response.getheader("location"))
#                except:
#                    raise PictureNotFoundError()

#                username = match.group(1)
#                id = match.group(2)
#        else:
#            username = param["username"]
#            id = param["id"]

#        table = "pick"
#        columns = ["id", "large_size"]
#        result = self.select_one(cursor, table, columns[1:], {columns[0]: id})
#        if result:
#            return result[0]

#        response = urllib2.build_opener(DontRedirectHandler).open(
#            "http://pick.naver.jp/%s/%s" % (username, id))

#        if response.getcode() == 302:
#            raise PictureNotFoundError()

#        parser = PickParser()
#        parser.feed(response.read())
#        parser.close()
#        large_size = parser.large_size

#        self.insert_all(cursor, table, (id, large_size))
#        return large_size

#    def get_full(self, match):
#        return self.work(match)

#    def get_full_https(self, match):
#        return None
    
#    def get_large(self, match):
#        return self.get_full(match)

#    def get_large_https(self, match):
#        return None

#    def get_thumb(self, match):
#        return self.get_full(match)

#    def get_thumb_https(self, match):
#        return None

#    def get_video(self, match):
#        return None

#    def get_video_https(self, match):
#        return None
