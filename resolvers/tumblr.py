# -*- coding: utf-8 -*-

import re
import os
import MySQLdb
import urllib2
import urlparse
import json
from private_constant import *

class tumblr:
	def __str__(self):
		return "Tumblr"
	
	regexStr = "^http://([\\w\\-]+\\.tumblr\\.com)/post/(\\d+)(?:/(?:[\\w\\-]+)?)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		hostname = match.group(1)
		id = match.group(2)
		
		if self.last is not None and str(self.last[0]) == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM tumblr WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://api.tumblr.com/v2/blog/%s/posts?api_key=%s&id=%s"
					% (hostname, tumblrApiKey, id))
			except Exception, ex:
				return None
			
			post = json.loads(httpRes.read())["response"]["posts"][0]
			
			original = ""
			large = ""
			thumb = ""
			video = ""
			
			type = post["type"]
			
			if type == "photo":
				photo = post["photos"][0]
				original = photo["original_size"]["url"]
				
				altSizes = photo["alt_sizes"]
				for size in altSizes:
					if size["width"] > 420 and size["width"] <= 500:
						large = size["url"]
				
				thumb = altSizes[-2]["url"]
			elif type == "video":
				original = large = thumb = post["thumbnail_url"]
				video = post["video_url"] if "video_url" in post else ""
			else:
				return None
			
			c.execute("INSERT INTO tumblr VALUES (%s, %s, %s, %s, %s)"
				% (db.literal(id), db.literal(original), db.literal(large), db.literal(thumb), db.literal(video))
			)
			
			db.commit()
			
			self.last = [id, original, large, thumb, video]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		
		if data is None:
			return None
		
		query = dict(urlparse.parse_qsl(os.environ.get("QUERY_STRING", "")))
		if "movie" in query and query["movie"] == "true" and data[4] != "":
			return data[4]
		else:
			return data[1]
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None
