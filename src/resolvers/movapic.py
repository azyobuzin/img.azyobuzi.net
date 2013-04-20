# -*- coding: utf-8 -*-

import re
##from sgmllib import SGMLParser

from resolvers import *

##image_regex = re.compile(r"^http://image\.movapic\.com/pic/m_(\w+)\.jpeg$")
##
##class MovapicParser(SGMLParser):
##    def __init__(self):
##        SGMLParser.__init__(self)
##        self.expanded = None
##
##    def do_img(self, attributes):
##        dic = dict(attributes)
##        if dic.get("class") == "image":
##            src = dic["src"]
##            match = image_regex.match(src)
##            if match:
##                self.expanded = match.group(1)

class Movapic(StoringResolver):
    @property
    def service_name(self):
        return u"携帯百景"

    @property
    def regex_str(self):
        return r"^http://(?:www\.)?movapic\.com/(?:(\w+)/pic/(\d+)|pic/(\w+))/?(?:\?.*)?$"

    image_regex = re.compile(r"""<img class="image" src="http://image\.movapic\.com/pic/m_(\w+)\.jpeg"/>""")

    def get_parameters(self, match):
        return {"username": match.group(1), "id": match.group(2)}

    def get_id(self, match):
        expanded = match.group(3)
        return expanded if expanded else self.work(match)

    def _work(self, param, cursor):
        table = "movapic"
        columns = ["id", "expanded"]
        result = self.select_one(cursor, table, columns[1:], {columns[0]: param["id"]})
        if result:
            return result[0]

        response = urllib2.urlopen("http://movapic.com/%(username)s/pic/%(id)s" % param)
        html = response.read().decode("utf-8")

##        parser = MovapicParser()
##        parser.feed(html)
##        parser.close()
##
##        expanded = parser.expanded

        match = self.image_regex.search(html)
        if not match:
            raise PictureNotFoundError()
        expanded = match.group(1)

        self.insert_all(cursor, table, (param["id"], expanded))
        return expanded

    def get_full(self, match):
        return "http://image.movapic.com/pic/m_%s.jpeg" % self.get_id(match)

    def get_full_https(self, match):
        return None

    def get_large(self, match):
        return self.get_full(match)

    def get_large_https(self, match):
        return None

    def get_thumb(self, match):
        return "http://image.movapic.com/pic/t_%s.jpeg" % self.get_id(match)

    def get_thumb_https(self, match):
        return None

    def get_video(self, match):
        return None

    def get_video_https(self, match):
        return None
