import { Component, h } from "preact";
import { Meme, MemeIdentification } from "../../models";
import ky from "ky";
import { route } from "preact-router";
import Loading from "../../components/Loading";


export default class LandingPage extends Component<any, any> {
    componentDidMount() {
        ky.get(`/api/memes`)
            .json<MemeIdentification>()
            .then(meme => route(`/${meme.threadId}/${meme.id}`));
    }

    render(){
        return (
            <Loading/>
        );
    }
}
