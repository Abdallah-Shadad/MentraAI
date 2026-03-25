from fastapi import FastAPI 
from routes import base
from utils.metrics import setup_metrics
from stores.graph   import GraphProviderFactory
from routes import roadmap

app = FastAPI()

setup_metrics(app)

async def startup_span():
    pass

async def shutdown_span():
    pass


app.on_event("startup")(startup_span)
app.on_event("shutdown")(shutdown_span)

app.include_router(base.base_router)
app.include_router(roadmap.roadmap_router)