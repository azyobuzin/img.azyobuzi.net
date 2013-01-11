# -*- coding: utf-8 -*-

import re
import os
import MySQLdb
import httplib
import urllib2
import urlparse
import json
from private_constant import *

class tumblr:
	def __str__(self):
		return "Tumblr"
	
	regexStr = r"^http://(?:([\w\-]+\.tumblr\.com)/(?:post|image)/(\d+)(?:/(?:[\w\-]+/?)?)?|tmblr\.co/([\w\-]+))(?:\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	expandedRegex = re.compile("^http://([\\w\\.\\-]+)/post/(\\d+)(?:/(?:[\\w\\-]+/?)?)?$", re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		hostname = match.group(1)
		id = match.group(2)
		shorten = match.group(3)
		
		db = None
		c = None
		
		if shorten is not None:
			db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
			c = db.cursor()
			
			c.execute("SELECT * FROM tumblr_shorten WHERE shorten = " + db.literal(shorten))
			sqlResult = c.fetchone()
			
			if sqlResult is None:
				conn = httplib.HTTPConnection("www.tumblr.com")
				conn.request("HEAD", "/" + shorten)
				res = conn.getresponse()
				exMatch = None
				try:
					exMatch = self.expandedRegex.match(res.getheader("location"))
				except:
					return None
				hostname = exMatch.group(1)
				id = exMatch.group(2)
				
				c.execute("INSERT INTO tumblr_shorten VALUES (%s, %s, %s)"
					% (db.literal(shorten), db.literal(hostname), db.literal(id))
				)
				
				db.commit()
			else:
				hostname = sqlResult[1]
				id = sqlResult[2]
		
		if self.last is not None and str(self.last[0]) == str(id):
			return self.last
		
		if db is None:
			db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
			c = db.cursor()
		
		c.execute("SELECT * FROM tumblr WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://api.tumblr.com/v2/blog/%s/posts?api_key=%s&id=%s"
					% (hostname, tumblrApiKey, id))
			except:
				return None
			
			post = json.loads(httpRes.read())["response"]["posts"][0]
			
			original = None
			large = None
			thumb = None
			video = ""
			
			type = post["type"]
			
			if type == "photo":
				photo = post["photos"][0]
				original = photo["original_size"]["url"]
				
				altSizes = photo["alt_sizes"]
				for size in altSizes:
					if size["width"] > 420 and size["width"] <= 500:
						large = size["url"]
				
				if large is None:
					large = original
				
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
