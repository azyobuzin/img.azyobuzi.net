# -*- coding: utf-8 -*-

import re
import MySQLdb
import httplib
import json
from private_constant import *

class cloudapp:
	def __str__(self):
		return "CloudApp"
	
	regexStr = "^https?://(?:www\\.)?cl\\.ly/(image/\\w+|\\w+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and self.last[0] == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM cloudapp WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			conn = httplib.HTTPConnection("cl.ly")
			conn.request("GET", "/" + id, headers = { "Accept": "application/json" })
			res = conn.getresponse()
			resStr = res.read()
			conn.close()
			
			if res.status == httplib.NOT_FOUND:
				return None
			
			j = json.loads(resStr)
			
			remote = j["remote_url"]
			thumbnail = j["thumbnail_url"]
			
			c.execute("INSERT INTO cloudapp VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(remote), db.literal(thumbnail))
			)
			
			db.commit()
			
			self.last = [id, remote, thumbnail]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
