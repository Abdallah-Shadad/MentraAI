from fastapi import FastAPI
from routes import base
from utils.metrics import setup_metrics
from stores.graph import GraphProviderFactory
from routes import roadmap
from routes.AdaptionEngine import adaptation_router
from routes.chat import chat_router, init_chat_router
from routes.quiz import quiz_router
from routes.project import project_router
from routes.track_recommender import track_recommender_router
from fastapi.middleware.cors import CORSMiddleware
app = FastAPI()

setup_metrics(app)


async def startup_span():
    # Initialise the semantic cache Redis vector index + ChatRouter internals
    await init_chat_router()


async def shutdown_span():
    pass

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.on_event("startup")(startup_span)
app.on_event("shutdown")(shutdown_span)

app.include_router(base.base_router)
app.include_router(roadmap.roadmap_router)
app.include_router(adaptation_router)
app.include_router(chat_router)
app.include_router(quiz_router)
app.include_router(project_router)
app.include_router(track_recommender_router)
