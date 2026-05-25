from fastapi import FastAPI 
from routes import base
from utils.metrics import setup_metrics
from stores.graph   import GraphProviderFactory
from routes import roadmap
from routes.AdaptionEngine import adaptation_router
from routes.quiz import quiz_router
from routes.project import project_router

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
app.include_router(adaptation_router)
app.include_router(quiz_router)
app.include_router(project_router)